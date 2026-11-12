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

        // GET: SeguimientoFisico/ProgresoFisico/5 (HU-19: Consultar progreso físico del cliente)
        public async Task<IActionResult> ProgresoFisico(int? id) // id del cliente (usuario_id)
        {
            // Si entran al módulo desde el menú principal sin un ID de cliente, 
            // cargamos la lista de clientes utilizando la vista "HistorialEvaluaciones" para evitar redundancias.
            if (id == null)
            {
                var clientes = await _context.Clientes
                    .Include(c => c.Usuario)
                    .ToListAsync();

                return View("HistorialEvaluaciones", clientes);
            }

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.UsuarioId == id);

            if (cliente == null) return NotFound();

            // Obtener el historial de evaluaciones físicas usando el DbSet correcto del contexto
            var evaluaciones = await _context.EvaluacionFisicas
                .Where(e => e.ClienteId == id)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            // Obtener las observaciones registradas por los entrenadores usando el DbSet correcto
            var observaciones = await _context.ObservacionEntrenadors
                .Include(o => o.Entrenador)
                .ThenInclude(e => e.Usuario)
                .Where(o => o.ClienteId == id)
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();

            ViewBag.Cliente = cliente;
            ViewBag.Evaluaciones = evaluaciones;
            ViewBag.Observaciones = observaciones;

            // Pasamos la lista de evaluaciones como Modelo para que la vista la reciba correctamente
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
                        var evaluacion = new EvaluacionFisica
                        {
                            ClienteId = model.ClienteId!.Value,
                            EntrenadorId = entrenadorId.Value,
                            Fecha = DateTime.Now,
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

        // GET: SeguimientoFisico/RegistrarEvaluacion/5
        public IActionResult RegistrarEvaluacion(int clienteId)
        {
            ViewBag.ClienteId = clienteId;
            return View();
        }

        // POST: SeguimientoFisico/RegistrarEvaluacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ENTRENADOR,Entrenador")]
        public async Task<IActionResult> RegistrarEvaluacion(int clienteId, decimal? peso, decimal? estatura, string medidas, string observaciones, string tipo)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(usuarioIdClaim, out int entrenadorId))
            {
                var nuevaEvaluacion = new EvaluacionFisica
                {
                    ClienteId = clienteId,
                    EntrenadorId = entrenadorId,
                    Fecha = DateTime.Now,
                    Peso = peso,
                    Estatura = estatura,
                    Medidas = medidas,
                    Observaciones = observaciones,
                    Tipo = tipo
                };

                _context.EvaluacionFisicas.Add(nuevaEvaluacion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ProgresoFisico), new { id = clienteId });
        }
    }
}