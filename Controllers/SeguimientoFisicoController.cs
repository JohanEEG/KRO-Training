using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KRO_Training_Performance.Controllers
{
    [Authorize]
    public class SeguimientoFisicoController : Controller
    {
        private readonly KroTrainingContext _context;

        public SeguimientoFisicoController(KroTrainingContext context)
        {
            _context = context;
        }

       
        // HU-50: Consultar historial de evaluaciones físicas
[HttpGet]
        [Authorize(Roles = "ENTRENADOR")]
        public async Task<IActionResult> ProgresoFisico(
    int? id,
    [FromQuery] FiltroHistorialEvaluacionesViewModel filtro)
        {
            var entrenadorId = await ObtenerEntrenadorEvaluacionAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            // Al entrar desde el menú, mostrar mis clientes activos.
            if (!id.HasValue)
            {
                var clientes = await _context.Clientes
                    .AsNoTracking()
                    .Include(c => c.Usuario)
                    .Where(c =>
                        c.EntrenadorId == entrenadorId.Value &&
                        c.Usuario.Estado == "ACTIVO" &&
                        c.Usuario.Rol.Nombre == "CLIENTE")
                    .OrderBy(c => c.Usuario.Nombre)
                    .ToListAsync();

                return View("HistorialEvaluaciones", clientes);
            }

            // Comprobar que el cliente pertenece al entrenador.
            var cliente = await _context.Clientes
                .AsNoTracking()
                .Include(c => c.Usuario)
                .SingleOrDefaultAsync(c =>
                    c.UsuarioId == id.Value &&
                    c.EntrenadorId == entrenadorId.Value &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE");

            if (cliente == null)
                return NotFound("El cliente no está disponible para su cuenta.");

            var consultaGeneral = _context.EvaluacionFisicas
                .AsNoTracking()
                .Where(e => e.ClienteId == cliente.UsuarioId);

            // Datos generales: no dependen del filtro de fechas.
            var totalEvaluaciones = await consultaGeneral.CountAsync();

            var ultimaEvaluacion = await consultaGeneral
                .OrderByDescending(e => e.Fecha)
                .ThenByDescending(e => e.EvaluacionId)
                .FirstOrDefaultAsync();

            var primeraEvaluacion = await consultaGeneral
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.EvaluacionId)
                .FirstOrDefaultAsync();

            var consultaFiltrada = consultaGeneral;

            if (ModelState.IsValid)
            {
                if (filtro.FechaInicio.HasValue)
                {
                    var desde = filtro.FechaInicio.Value.Date;

                    consultaFiltrada = consultaFiltrada
                        .Where(e => e.Fecha >= desde);
                }

                if (filtro.FechaFin.HasValue)
                {
                    var hasta = filtro.FechaFin.Value.Date;

                    // Incluir todas las horas del día final.
                    // Evitar desbordamiento si se recibe 31/12/9999.
                    if (hasta < DateTime.MaxValue.Date)
                    {
                        var hastaExclusivo = hasta.AddDays(1);

                        consultaFiltrada = consultaFiltrada
                            .Where(e => e.Fecha < hastaExclusivo);
                    }
                }
            }
            else
            {
                // No mostrar resultados de un filtro inválido.
                consultaFiltrada = consultaFiltrada.Where(e => false);
            }

            var evaluaciones = await consultaFiltrada
                .OrderByDescending(e => e.Fecha)
                .ThenByDescending(e => e.EvaluacionId)
                .ToListAsync();

            var observaciones = await _context.ObservacionEntrenadors
                .AsNoTracking()
                .Include(o => o.Entrenador)
                    .ThenInclude(e => e.Usuario)
                .Where(o => o.ClienteId == cliente.UsuarioId)
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();

            ViewBag.Cliente = cliente;
            ViewBag.Evaluaciones = evaluaciones;
            ViewBag.Observaciones = observaciones;

            ViewBag.Filtro = filtro;
            ViewBag.TotalEvaluaciones = totalEvaluaciones;
            ViewBag.UltimaEvaluacion = ultimaEvaluacion;
            ViewBag.PrimeraEvaluacion = primeraEvaluacion;

            ViewBag.FiltroInvalido = !ModelState.IsValid;

            ViewBag.HayFiltro =
                filtro.FechaInicio.HasValue ||
                filtro.FechaFin.HasValue;

            return View(evaluaciones);
        }

        // POST: SeguimientoFisico/RegistrarObservacion (HU-81: Registrar observaciones como entrenador)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ENTRENADOR,Entrenador")]
        public async Task<IActionResult> RegistrarObservacion(int clienteId, string contenido)
        {
            if (!string.IsNullOrWhiteSpace(contenido))
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(usuarioIdClaim, out int entrenadorId))
                {
                    var nuevaObservacion = new ObservacionEntrenador
                    {
                        ClienteId = clienteId,
                        EntrenadorId = entrenadorId,
                        Fecha = DateTime.Now,
                        Contenido = contenido
                    };

                    _context.ObservacionEntrenadors.Add(nuevaObservacion);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(ProgresoFisico), new { id = clienteId });
        }

        // HU-48: Abrir el formulario de evaluación inicial
        [HttpGet]
        [Authorize(Roles = "ENTRENADOR")]
        public async Task<IActionResult> RegistrarEvaluacionInicial()
        {
            var entrenadorId = await ObtenerEntrenadorEvaluacionAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            var model = new RegistrarEvaluacionInicialViewModel();

            await CargarClientesEvaluacionAsync(model, entrenadorId.Value);

            return View(model);
        }

        // HU-48: Guardar la evaluación inicial
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ENTRENADOR")]
        public async Task<IActionResult> RegistrarEvaluacionInicial(
            RegistrarEvaluacionInicialViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorEvaluacionAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            model.Medidas = string.IsNullOrWhiteSpace(model.Medidas)
                ? null
                : model.Medidas.Trim();

            model.Observaciones = string.IsNullOrWhiteSpace(model.Observaciones)
                ? null
                : model.Observaciones.Trim();

            // La base de datos guarda dos decimales.
            if (model.Peso.HasValue &&
                decimal.Round(model.Peso.Value, 2) != model.Peso.Value)
            {
                ModelState.AddModelError(
                    nameof(model.Peso),
                    "El peso puede tener como máximo dos decimales.");
            }

            if (model.Estatura.HasValue &&
                decimal.Round(model.Estatura.Value, 2) != model.Estatura.Value)
            {
                ModelState.AddModelError(
                    nameof(model.Estatura),
                    "La estatura puede tener como máximo dos decimales.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await using var transaccion =
                        await _context.Database.BeginTransactionAsync(
                            System.Data.IsolationLevel.Serializable);

                    var clienteValido = await _context.Clientes
                        .AnyAsync(c =>
                            c.UsuarioId == model.ClienteId!.Value &&
                            c.EntrenadorId == entrenadorId.Value &&
                            c.Usuario.Estado == "ACTIVO" &&
                            c.Usuario.Rol.Nombre == "CLIENTE");

                    if (!clienteValido)
                    {
                        ModelState.AddModelError(
                            nameof(model.ClienteId),
                            "Seleccione un cliente activo asignado a usted.");
                    }
                    else
                    {
                        var tieneInicial = await _context.EvaluacionFisicas
                            .AnyAsync(e =>
                                e.ClienteId == model.ClienteId!.Value &&
                                e.Tipo != null &&
                                e.Tipo.Trim().ToUpper() == "INICIAL");

                        if (tieneInicial)
                        {
                            ModelState.AddModelError(
                                nameof(model.ClienteId),
                                "El cliente ya tiene una evaluación inicial registrada.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        var fechaRegistroInicial = DateTime.Now;
                        var inicioDiaInicial = fechaRegistroInicial.Date;
                        var finDiaInicial = inicioDiaInicial.AddDays(1);

                        var existeEvaluacionEnFecha = await _context.EvaluacionFisicas
                            .AnyAsync(e =>
                                e.ClienteId == model.ClienteId!.Value &&
                                e.Fecha >= inicioDiaInicial &&
                                e.Fecha < finDiaInicial);

                        if (existeEvaluacionEnFecha)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                "El cliente ya tiene una evaluación registrada hoy. " +
                                "No se puede registrar otra en la misma fecha.");

                            await transaccion.RollbackAsync();

                            await CargarClientesEvaluacionAsync(
                                model, entrenadorId.Value);

                            return View(model);
                        }

                        var evaluacion = new EvaluacionFisica
                        {
                            ClienteId = model.ClienteId!.Value,
                            EntrenadorId = entrenadorId.Value,
                            Fecha = fechaRegistroInicial,
                            Peso = model.Peso,
                            Estatura = model.Estatura,
                            Medidas = model.Medidas,
                            Observaciones = model.Observaciones,
                            Tipo = "Inicial"
                        };

                        _context.EvaluacionFisicas.Add(evaluacion);

                        await _context.SaveChangesAsync();
                        await transaccion.CommitAsync();

                        TempData["MensajeExito"] =
                            "La evaluación física inicial se registró correctamente.";

                        return RedirectToAction(
                            nameof(RegistrarEvaluacionInicial));
                    }

                    await transaccion.RollbackAsync();
                }
                catch (Exception ex) when (
                    ex is DbUpdateException ||
                    ex is Microsoft.Data.SqlClient.SqlException)
                {
                    var logger = HttpContext.RequestServices
                        .GetRequiredService<
                            Microsoft.Extensions.Logging.ILogger<
                                SeguimientoFisicoController>>();

                    logger.LogError(
                        ex,
                        "Error al registrar la evaluación inicial del cliente {ClienteId}.",
                        model.ClienteId);

                    ModelState.AddModelError(
                        string.Empty,
                        "No se pudo guardar la evaluación. Intente nuevamente.");
                }
            }

            await CargarClientesEvaluacionAsync(model, entrenadorId.Value);

            return View(model);
        }

        private async Task<int?> ObtenerEntrenadorEvaluacionAsync()
        {
            var identificador =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificador, out var usuarioId))
                return null;

            var entrenadorValido = await _context.Entrenadors
                .AsNoTracking()
                .AnyAsync(e =>
                    e.UsuarioId == usuarioId &&
                    e.Usuario.Estado == "ACTIVO" &&
                    e.Usuario.Rol.Nombre == "ENTRENADOR");

            return entrenadorValido ? usuarioId : null;
        }

        private async Task CargarClientesEvaluacionAsync(
            RegistrarEvaluacionInicialViewModel model,
            int entrenadorId)
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.EntrenadorId == entrenadorId &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE")
                .OrderBy(c => c.Usuario.Nombre)
                .Select(c => new
                {
                    c.UsuarioId,
                    c.Usuario.Nombre,
                    c.Usuario.Correo
                })
                .ToListAsync();

            model.ClientesDisponibles = clientes
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.UsuarioId.ToString(),
                    Text = $"{c.Nombre} ({c.Correo})"
                })
                .ToList();
        }

        // HU-49: Abrir formulario de evaluación periódica
        [HttpGet]
        [Authorize(Roles = "ENTRENADOR")]
        public async Task<IActionResult> RegistrarEvaluacion(int clienteId)
        {
            var entrenadorId = await ObtenerEntrenadorEvaluacionAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.UsuarioId == clienteId &&
                    c.EntrenadorId == entrenadorId.Value &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE")
                .Select(c => new
                {
                    c.UsuarioId,
                    c.Usuario.Nombre
                })
                .SingleOrDefaultAsync();

            if (cliente == null)
                return NotFound("El cliente no está disponible para su cuenta.");

            var model = new RegistrarEvaluacionPeriodicaViewModel
            {
                ClienteId = cliente.UsuarioId,
                NombreCliente = cliente.Nombre
            };

            return View(model);
        }

        // HU-49: Guardar evaluación periódica
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ENTRENADOR")]
        public async Task<IActionResult> RegistrarEvaluacion(
            RegistrarEvaluacionPeriodicaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorEvaluacionAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            model.Medidas = string.IsNullOrWhiteSpace(model.Medidas)
                ? null
                : model.Medidas.Trim();

            model.Observaciones = string.IsNullOrWhiteSpace(model.Observaciones)
                ? null
                : model.Observaciones.Trim();

            if (model.Peso.HasValue &&
                decimal.Round(model.Peso.Value, 2) != model.Peso.Value)
            {
                ModelState.AddModelError(
                    nameof(model.Peso),
                    "El peso puede tener como máximo dos decimales.");
            }

            if (model.Estatura.HasValue &&
                decimal.Round(model.Estatura.Value, 2) != model.Estatura.Value)
            {
                ModelState.AddModelError(
                    nameof(model.Estatura),
                    "La estatura puede tener como máximo dos decimales.");
            }

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable);

                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.UsuarioId == model.ClienteId &&
                        c.EntrenadorId == entrenadorId.Value &&
                        c.Usuario.Estado == "ACTIVO" &&
                        c.Usuario.Rol.Nombre == "CLIENTE")
                    .Select(c => new
                    {
                        c.UsuarioId,
                        c.Usuario.Nombre
                    })
                    .SingleOrDefaultAsync();

                if (cliente == null)
                    return NotFound("El cliente no está disponible para su cuenta.");

                model.NombreCliente = cliente.Nombre;

                if (!ModelState.IsValid)
                    return View(model);

                var fechaRegistro = DateTime.Now;
                var inicioDia = fechaRegistro.Date;
                var finDia = inicioDia.AddDays(1);

                var existeEvaluacionHoy = await _context.EvaluacionFisicas
                    .AnyAsync(e =>
                        e.ClienteId == model.ClienteId &&
                        e.Fecha >= inicioDia &&
                        e.Fecha < finDia);

                if (existeEvaluacionHoy)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "El cliente ya tiene una evaluación registrada hoy. " +
                        "No se puede registrar otra en la misma fecha.");

                    return View(model);
                }

                var evaluacion = new EvaluacionFisica
                {
                    ClienteId = cliente.UsuarioId,
                    EntrenadorId = entrenadorId.Value,
                    Fecha = fechaRegistro,
                    Peso = model.Peso,
                    Estatura = model.Estatura,
                    Medidas = model.Medidas,
                    Observaciones = model.Observaciones,
                    Tipo = "Periódica"
                };

                _context.EvaluacionFisicas.Add(evaluacion);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    "La evaluación física periódica se registró correctamente.";

                return RedirectToAction(
                    nameof(ProgresoFisico),
                    new { id = model.ClienteId });
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                var logger = HttpContext.RequestServices
                    .GetRequiredService<
                        Microsoft.Extensions.Logging.ILogger<
                            SeguimientoFisicoController>>();

                logger.LogError(
                    ex,
                    "Error al registrar la evaluación periódica del cliente {ClienteId}.",
                    model.ClienteId);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar la evaluación. Intente nuevamente.");

                return View(model);
            }
        }
    }
}