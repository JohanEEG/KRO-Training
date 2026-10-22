using System.Data;
using System.Security.Claims;
using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KROTraining.Controllers
{
    [Authorize(Roles = "ENTRENADOR")]
    public class RutinasController : Controller
    {
        private readonly KroTrainingContext _context;
        private readonly ILogger<RutinasController> _logger;

        public RutinasController(
            KroTrainingContext context,
            ILogger<RutinasController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            var rutinas = await _context.Rutinas
                .AsNoTracking()
                .Where(r => r.EntrenadorId == entrenadorId.Value)
                .OrderByDescending(r => r.FechaCreacion)
                .ThenByDescending(r => r.RutinaId)
                .ToListAsync();

            return View(rutinas);
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            return View(new CrearRutinaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            CrearRutinaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            model.Nombre = (model.Nombre ?? string.Empty).Trim();
            model.Descripcion = LimpiarOpcional(model.Descripcion);
            model.Objetivo = LimpiarOpcional(model.Objetivo);
            model.Nivel = LimpiarOpcional(model.Nivel)?.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError(
                    nameof(model.Nombre),
                    "Ingrese el nombre de la rutina.");
            }

            if (model.Nivel != null &&
                model.Nivel != "PRINCIPIANTE" &&
                model.Nivel != "INTERMEDIO" &&
                model.Nivel != "AVANZADO")
            {
                ModelState.AddModelError(
                    nameof(model.Nivel),
                    "Seleccione un nivel válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Mantiene juntas la comprobación y la creación,
                // incluso cuando hay solicitudes simultáneas.
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var nombreComparacion = model.Nombre.ToUpperInvariant();

                var nombreDuplicado = await _context.Rutinas
                    .AnyAsync(r =>
                        r.Nombre.Trim().ToUpper() == nombreComparacion);

                if (nombreDuplicado)
                {
                    ModelState.AddModelError(
                        nameof(model.Nombre),
                        "Ya existe una rutina con ese nombre.");

                    return View(model);
                }

                var rutina = new Rutina
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Objetivo = model.Objetivo,
                    Nivel = model.Nivel,
                    EntrenadorId = entrenadorId.Value,
                    FechaCreacion = DateTime.Now,
                    Estado = "ACTIVA"
                };

                _context.Rutinas.Add(rutina);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    "La rutina se creó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al crear una rutina para el entrenador {EntrenadorId}.",
                    entrenadorId.Value);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar la rutina. Intente nuevamente.");

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AgregarEjercicio(int id)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            var rutina = await _context.Rutinas
                .AsNoTracking()
                .SingleOrDefaultAsync(r =>
                    r.RutinaId == id &&
                    r.EntrenadorId == entrenadorId.Value);

            if (rutina == null)
            {
                return NotFound("La rutina no está disponible.");
            }

            var model = new AgregarEjercicioRutinaViewModel
            {
                RutinaId = rutina.RutinaId,
                NombreRutina = rutina.Nombre
            };

            await CargarEjerciciosDisponiblesAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarEjercicio(
            int id,
            AgregarEjercicioRutinaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            if (id <= 0 || id != model.RutinaId)
            {
                return BadRequest("El identificador de la rutina no es válido.");
            }

            var rutina = await _context.Rutinas
                .AsNoTracking()
                .SingleOrDefaultAsync(r =>
                    r.RutinaId == id &&
                    r.EntrenadorId == entrenadorId.Value);

            if (rutina == null)
            {
                return NotFound("La rutina no está disponible.");
            }

            model.NombreRutina = rutina.Nombre;

            if (!ModelState.IsValid)
            {
                await CargarEjerciciosDisponiblesAsync(model);
                return View(model);
            }

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var ejercicioId = model.EjercicioId!.Value;

                var existeEjercicio = await _context.Ejercicios
                    .AnyAsync(e => e.EjercicioId == ejercicioId);

                var yaAgregado = await _context.RutinaEjercicios
                    .AnyAsync(re =>
                        re.RutinaId == id &&
                        re.EjercicioId == ejercicioId);

                if (!existeEjercicio || yaAgregado)
                {
                    ModelState.AddModelError(
                        nameof(model.EjercicioId),
                        "El ejercicio no está disponible o ya pertenece a esta rutina.");
                }
                else
                {
                    var ultimoOrden = await _context.RutinaEjercicios
                        .Where(re => re.RutinaId == id)
                        .Select(re => (int?)re.Orden)
                        .MaxAsync() ?? 0;

                    if (ultimoOrden == int.MaxValue)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "No se pueden agregar más ejercicios a esta rutina.");
                    }
                    else
                    {
                        var detalle = new RutinaEjercicio
                        {
                            RutinaId = id,
                            EjercicioId = ejercicioId,
                            Orden = ultimoOrden + 1,
                            Series = model.Series!.Value,
                            Repeticiones = model.Repeticiones!.Value,
                            DescansoSegundos = model.DescansoSegundos!.Value
                        };

                        _context.RutinaEjercicios.Add(detalle);

                        await _context.SaveChangesAsync();
                        await transaccion.CommitAsync();

                        TempData["MensajeExito"] =
                            "El ejercicio se agregó correctamente a la rutina.";

                        return RedirectToAction(
                            nameof(AgregarEjercicio),
                            new { id });
                    }
                }

                await transaccion.RollbackAsync();
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al agregar el ejercicio {EjercicioId} a la rutina {RutinaId}.",
                    model.EjercicioId,
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo agregar el ejercicio. Actualice la página y vuelva a intentarlo.");
            }

            await CargarEjerciciosDisponiblesAsync(model);

            return View(model);
        }

        private async Task CargarEjerciciosDisponiblesAsync(
            AgregarEjercicioRutinaViewModel model)
        {
            var ejercicios = await _context.Ejercicios
                .AsNoTracking()
                .Where(e => !_context.RutinaEjercicios.Any(re =>
                    re.RutinaId == model.RutinaId &&
                    re.EjercicioId == e.EjercicioId))
                .OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    e.EjercicioId,
                    e.Nombre
                })
                .ToListAsync();

            model.EjerciciosDisponibles = ejercicios
                .Select(e => new SelectListItem
                {
                    Value = e.EjercicioId.ToString(),
                    Text = e.Nombre
                })
                .ToList();
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            var rutina = await _context.Rutinas
                .AsNoTracking()
                .Include(r => r.RutinaEjercicios)
                    .ThenInclude(re => re.Ejercicio)
                .SingleOrDefaultAsync(r =>
                    r.RutinaId == id &&
                    r.EntrenadorId == entrenadorId.Value);

            if (rutina == null)
            {
                return NotFound("La rutina no fue encontrada o no está disponible para su cuenta.");
            }

            var model = new EditarRutinaViewModel
            {
                RutinaId = rutina.RutinaId,
                Nombre = rutina.Nombre,
                Descripcion = rutina.Descripcion,
                Objetivo = rutina.Objetivo,
                Nivel = rutina.Nivel,
                Ejercicios = rutina.RutinaEjercicios
                    .OrderBy(re => re.Orden)
                    .Select(re => new EditarEjercicioRutinaViewModel
                    {
                        EjercicioId = re.EjercicioId,
                        NombreEjercicio = re.Ejercicio.Nombre,
                        Orden = re.Orden,
                        Series = re.Series,
                        Repeticiones = re.Repeticiones,
                        DescansoSegundos = re.DescansoSegundos
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            EditarRutinaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            if (id <= 0 || id != model.RutinaId)
            {
                return BadRequest("El identificador de la rutina no es válido.");
            }

            model.Nombre = (model.Nombre ?? string.Empty).Trim();
            model.Descripcion = LimpiarOpcional(model.Descripcion);
            model.Objetivo = LimpiarOpcional(model.Objetivo);
            model.Nivel = LimpiarOpcional(model.Nivel)?.ToUpperInvariant();

            model.Ejercicios ??= new List<EditarEjercicioRutinaViewModel>();

            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError(
                    nameof(model.Nombre),
                    "Ingrese el nombre de la rutina.");
            }

            if (model.Nivel != null &&
                model.Nivel != "PRINCIPIANTE" &&
                model.Nivel != "INTERMEDIO" &&
                model.Nivel != "AVANZADO")
            {
                ModelState.AddModelError(
                    nameof(model.Nivel),
                    "Seleccione un nivel válido.");
            }

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var rutina = await _context.Rutinas
                    .Include(r => r.RutinaEjercicios)
                        .ThenInclude(re => re.Ejercicio)
                    .SingleOrDefaultAsync(r =>
                        r.RutinaId == id &&
                        r.EntrenadorId == entrenadorId.Value);

                if (rutina == null)
                {
                    return NotFound(
                        "La rutina no fue encontrada o no está disponible para su cuenta.");
                }

                var detalles = rutina.RutinaEjercicios
                    .ToDictionary(re => re.EjercicioId);

                var idsEnviados = model.Ejercicios
                    .Select(e => e.EjercicioId)
                    .ToList();

                // No permite agregar, quitar o sustituir ejercicios
                // manipulando los campos del formulario de edición.
                var listaValida =
                    idsEnviados.Count == detalles.Count &&
                    idsEnviados.Distinct().Count() == idsEnviados.Count &&
                    idsEnviados.All(ejercicioId =>
                        detalles.ContainsKey(ejercicioId));

                if (!listaValida)
                {
                    return BadRequest(
                        "La lista de ejercicios cambió o no es válida. " +
                        "Vuelva al listado y abra nuevamente la edición.");
                }

                // Los nombres y el orden siempre se obtienen de la base.
                foreach (var ejercicio in model.Ejercicios)
                {
                    var detalle = detalles[ejercicio.EjercicioId];

                    ejercicio.NombreEjercicio = detalle.Ejercicio.Nombre;
                    ejercicio.Orden = detalle.Orden;
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var nombreComparacion = model.Nombre.ToUpperInvariant();

                var nombreDuplicado = await _context.Rutinas
                    .AnyAsync(r =>
                        r.RutinaId != id &&
                        r.Nombre.Trim().ToUpper() == nombreComparacion);

                if (nombreDuplicado)
                {
                    ModelState.AddModelError(
                        nameof(model.Nombre),
                        "Ya existe otra rutina con ese nombre.");

                    return View(model);
                }

                var datosGeneralesCambiaron =
                    rutina.Nombre != model.Nombre ||
                    rutina.Descripcion != model.Descripcion ||
                    rutina.Objetivo != model.Objetivo ||
                    rutina.Nivel != model.Nivel;

                var ejerciciosCambiaron = model.Ejercicios.Any(e =>
                {
                    var detalle = detalles[e.EjercicioId];

                    return detalle.Series != e.Series!.Value ||
                           detalle.Repeticiones != e.Repeticiones!.Value ||
                           detalle.DescansoSegundos != e.DescansoSegundos!.Value;
                });

                if (!datosGeneralesCambiaron && !ejerciciosCambiaron)
                {
                    ViewData["MensajeInformativo"] =
                        "No realizó cambios en la rutina.";

                    return View(model);
                }

                rutina.Nombre = model.Nombre;
                rutina.Descripcion = model.Descripcion;
                rutina.Objetivo = model.Objetivo;
                rutina.Nivel = model.Nivel;

                foreach (var ejercicio in model.Ejercicios)
                {
                    var detalle = detalles[ejercicio.EjercicioId];

                    detalle.Series = ejercicio.Series!.Value;
                    detalle.Repeticiones = ejercicio.Repeticiones!.Value;
                    detalle.DescansoSegundos = ejercicio.DescansoSegundos!.Value;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    "La rutina se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al editar la rutina {RutinaId}.",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron guardar los cambios. " +
                    "Vuelva al listado y abra nuevamente la edición.");

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            var model = await ObtenerDatosEliminacionAsync(
                id,
                entrenadorId.Value);

            if (model == null)
            {
                return NotFound(
                    "La rutina no fue encontrada o no está disponible para su cuenta.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(
            int id,
            EliminarRutinaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (entrenadorId == null)
            {
                return Forbid();
            }

            if (id <= 0 || id != model.RutinaId)
            {
                return BadRequest(
                    "El identificador de la rutina no es válido.");
            }

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                // Consulta nuevamente las relaciones antes de eliminar.
                var datos = await ObtenerDatosEliminacionAsync(
                    id,
                    entrenadorId.Value);

                if (datos == null)
                {
                    return NotFound(
                        "La rutina no fue encontrada o no está disponible para su cuenta.");
                }

                model.Nombre = datos.Nombre;
                model.Objetivo = datos.Objetivo;
                model.Nivel = datos.Nivel;
                model.CantidadEjercicios = datos.CantidadEjercicios;
                model.TieneAsignaciones = datos.TieneAsignaciones;
                model.TieneRecomendaciones = datos.TieneRecomendaciones;

                if (model.TieneAsignaciones)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "No se puede eliminar la rutina porque tiene asignaciones a clientes.");
                }

                if (model.TieneRecomendaciones)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "No se puede eliminar la rutina porque tiene recomendaciones asociadas.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Elimina únicamente las asociaciones de esta rutina.
                // Los ejercicios del catálogo se conservan.
                await _context.RutinaEjercicios
                    .Where(re => re.RutinaId == id)
                    .ExecuteDeleteAsync();

                var eliminadas = await _context.Rutinas
                    .Where(r =>
                        r.RutinaId == id &&
                        r.EntrenadorId == entrenadorId.Value)
                    .ExecuteDeleteAsync();

                if (eliminadas != 1)
                {
                    await transaccion.RollbackAsync();

                    return NotFound(
                        "La rutina no fue encontrada o ya no está disponible.");
                }

                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    "La rutina se eliminó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar la rutina {RutinaId}.",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo eliminar la rutina. " +
                    "Vuelva al listado y abra nuevamente la confirmación.");

                return View(model);
            }
        }

        private async Task<EliminarRutinaViewModel?> ObtenerDatosEliminacionAsync(
            int rutinaId,
            int entrenadorId)
        {
            return await _context.Rutinas
                .AsNoTracking()
                .Where(r =>
                    r.RutinaId == rutinaId &&
                    r.EntrenadorId == entrenadorId)
                .Select(r => new EliminarRutinaViewModel
                {
                    RutinaId = r.RutinaId,
                    Nombre = r.Nombre,
                    Objetivo = r.Objetivo,
                    Nivel = r.Nivel,
                    CantidadEjercicios = r.RutinaEjercicios.Count,
                    TieneAsignaciones = r.AsignacionRutinas.Any(),
                    TieneRecomendaciones = r.Recomendacions.Any()
                })
                .SingleOrDefaultAsync();
        }

        // HU-45: Asignar rutina a cliente
        [HttpGet]
        public async Task<IActionResult> Asignar(int id)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            var model = await _context.Rutinas
                .AsNoTracking()
                .Where(r =>
                    r.RutinaId == id &&
                    r.EntrenadorId == entrenadorId.Value &&
                    r.Estado == "ACTIVA")
                .Select(r => new AsignarRutinaViewModel
                {
                    RutinaId = r.RutinaId,
                    NombreRutina = r.Nombre,
                    ObjetivoRutina = r.Objetivo,
                    NivelRutina = r.Nivel
                })
                .SingleOrDefaultAsync();

            if (model == null)
                return NotFound("La rutina no existe o no está disponible.");

            await CargarClientesParaAsignacionAsync(
                model, entrenadorId.Value);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(
            int id,
            AsignarRutinaViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            if (id != model.RutinaId)
                return BadRequest("El identificador de la rutina no coincide.");

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable);

                var rutina = await _context.Rutinas
                    .AsNoTracking()
                    .Where(r =>
                        r.RutinaId == id &&
                        r.EntrenadorId == entrenadorId.Value &&
                        r.Estado == "ACTIVA")
                    .Select(r => new
                    {
                        r.Nombre,
                        r.Objetivo,
                        r.Nivel
                    })
                    .SingleOrDefaultAsync();

                if (rutina == null)
                    return NotFound("La rutina no existe o no está disponible.");

                model.NombreRutina = rutina.Nombre;
                model.ObjetivoRutina = rutina.Objetivo;
                model.NivelRutina = rutina.Nivel;

                if (ModelState.IsValid)
                {
                    var clienteValido = await _context.Clientes
                        .AsNoTracking()
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
                        var yaAsignada = await _context.AsignacionRutinas
                            .AnyAsync(a =>
                                a.ClienteId == model.ClienteId!.Value &&
                                a.RutinaId == id &&
                                a.Estado == "ACTIVA");

                        if (yaAsignada)
                        {
                            ModelState.AddModelError(
                                nameof(model.ClienteId),
                                "El cliente ya tiene esta rutina activa.");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var asignacion = new AsignacionRutina
                    {
                        ClienteId = model.ClienteId!.Value,
                        RutinaId = id,
                        EntrenadorId = entrenadorId.Value,
                        FechaAsignacion = DateTime.Now,
                        Estado = "ACTIVA"
                    };

                    _context.AsignacionRutinas.Add(asignacion);

                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData["MensajeExito"] =
                        "La rutina se asignó correctamente al cliente.";

                    return RedirectToAction(nameof(Index));
                }

                await transaccion.RollbackAsync();
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al asignar la rutina {RutinaId} al cliente {ClienteId}.",
                    id,
                    model.ClienteId);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo asignar la rutina. Intente nuevamente.");
            }

            await CargarClientesParaAsignacionAsync(
                model, entrenadorId.Value);

            return View(model);
        }

        private async Task CargarClientesParaAsignacionAsync(
            AsignarRutinaViewModel model,
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

        // HU-46: Consultar asignaciones de mis clientes
        [HttpGet]
        public async Task<IActionResult> Asignaciones()
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.EntrenadorId == entrenadorId.Value &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE")
                .OrderBy(c => c.Usuario.Nombre)
                .Select(c => new
                {
                    ClienteId = c.UsuarioId,
                    Nombre = c.Usuario.Nombre,
                    Correo = c.Usuario.Correo
                })
                .ToListAsync();

            var asignaciones = await _context.AsignacionRutinas
                .AsNoTracking()
                .Where(a =>
                    a.Estado == "ACTIVA" &&
                    a.Cliente.EntrenadorId == entrenadorId.Value &&
                    a.Cliente.Usuario.Estado == "ACTIVO" &&
                    a.Cliente.Usuario.Rol.Nombre == "CLIENTE")
                .OrderByDescending(a => a.FechaAsignacion)
                .ThenByDescending(a => a.AsignacionId)
                .Select(a => new
                {
                    a.ClienteId,
                    a.AsignacionId,
                    NombreRutina = a.Rutina.Nombre,
                    a.FechaAsignacion
                })
                .ToListAsync();

            var porCliente = asignaciones.ToLookup(a => a.ClienteId);
            var model = new List<ClienteAsignacionItemViewModel>();

            foreach (var cliente in clientes)
            {
                var asignacionesCliente = porCliente[cliente.ClienteId];

                if (!asignacionesCliente.Any())
                {
                    model.Add(new ClienteAsignacionItemViewModel
                    {
                        ClienteId = cliente.ClienteId,
                        NombreCliente = cliente.Nombre,
                        CorreoCliente = cliente.Correo
                    });

                    continue;
                }

                foreach (var asignacion in asignacionesCliente)
                {
                    model.Add(new ClienteAsignacionItemViewModel
                    {
                        ClienteId = cliente.ClienteId,
                        NombreCliente = cliente.Nombre,
                        CorreoCliente = cliente.Correo,
                        AsignacionId = asignacion.AsignacionId,
                        NombreRutina = asignacion.NombreRutina,
                        FechaAsignacion = asignacion.FechaAsignacion
                    });
                }
            }

            return View(model);
        }

        // HU-46: Abrir formulario de modificación
        [HttpGet]
        public async Task<IActionResult> ModificarAsignacion(int id)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            var model = await _context.AsignacionRutinas
                .AsNoTracking()
                .Where(a =>
                    a.AsignacionId == id &&
                    a.Estado == "ACTIVA" &&
                    a.Cliente.EntrenadorId == entrenadorId.Value &&
                    a.Cliente.Usuario.Estado == "ACTIVO" &&
                    a.Cliente.Usuario.Rol.Nombre == "CLIENTE")
                .Select(a => new ModificarAsignacionViewModel
                {
                    AsignacionId = a.AsignacionId,
                    NombreCliente = a.Cliente.Usuario.Nombre,
                    NombreRutinaActual = a.Rutina.Nombre
                })
                .SingleOrDefaultAsync();

            if (model == null)
                return NotFound("La asignación no existe o ya no está disponible.");

            await CargarRutinasParaCambioAsync(model, entrenadorId.Value);

            return View(model);
        }

        // HU-46: Guardar modificación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarAsignacion(
            int id,
            ModificarAsignacionViewModel model)
        {
            var entrenadorId = await ObtenerEntrenadorActivoAsync();

            if (!entrenadorId.HasValue)
                return Forbid();

            if (id <= 0 || id != model.AsignacionId)
                return BadRequest("El identificador de la asignación no es válido.");

            try
            {
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var asignacion = await _context.AsignacionRutinas
                    .SingleOrDefaultAsync(a =>
                        a.AsignacionId == id &&
                        a.Estado == "ACTIVA" &&
                        a.Cliente.EntrenadorId == entrenadorId.Value &&
                        a.Cliente.Usuario.Estado == "ACTIVO" &&
                        a.Cliente.Usuario.Rol.Nombre == "CLIENTE");

                if (asignacion == null)
                    return NotFound("La asignación no existe o ya no está disponible.");

                model.NombreCliente = await _context.Clientes
                    .Where(c => c.UsuarioId == asignacion.ClienteId)
                    .Select(c => c.Usuario.Nombre)
                    .SingleAsync();

                model.NombreRutinaActual = await _context.Rutinas
                    .Where(r => r.RutinaId == asignacion.RutinaId)
                    .Select(r => r.Nombre)
                    .SingleAsync();

                if (ModelState.IsValid)
                {
                    var nuevaRutinaId = model.NuevaRutinaId!.Value;

                    if (nuevaRutinaId == asignacion.RutinaId)
                    {
                        ModelState.AddModelError(
                            nameof(model.NuevaRutinaId),
                            "Seleccione una rutina diferente de la actual.");
                    }
                    else
                    {
                        var rutinaDisponible = await _context.Rutinas
                            .AnyAsync(r =>
                                r.RutinaId == nuevaRutinaId &&
                                r.EntrenadorId == entrenadorId.Value &&
                                r.Estado == "ACTIVA");

                        if (!rutinaDisponible)
                        {
                            ModelState.AddModelError(
                                nameof(model.NuevaRutinaId),
                                "La rutina seleccionada no está disponible.");
                        }
                        else
                        {
                            var yaAsignada = await _context.AsignacionRutinas
                                .AnyAsync(a =>
                                    a.ClienteId == asignacion.ClienteId &&
                                    a.RutinaId == nuevaRutinaId &&
                                    a.Estado == "ACTIVA");

                            if (yaAsignada)
                            {
                                ModelState.AddModelError(
                                    nameof(model.NuevaRutinaId),
                                    "El cliente ya tiene esa rutina activa.");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    // Conserva la asignación anterior y sus sesiones.
                    asignacion.Estado = "INACTIVA";

                    _context.AsignacionRutinas.Add(new AsignacionRutina
                    {
                        ClienteId = asignacion.ClienteId,
                        RutinaId = model.NuevaRutinaId!.Value,
                        EntrenadorId = entrenadorId.Value,
                        FechaAsignacion = DateTime.Now,
                        Estado = "ACTIVA"
                    });

                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData["MensajeExito"] =
                        "La rutina asignada se modificó correctamente.";

                    return RedirectToAction(nameof(Asignaciones));
                }

                await transaccion.RollbackAsync();
            }
            catch (Exception ex) when (
                ex is DbUpdateException ||
                ex is Microsoft.Data.SqlClient.SqlException)
            {
                _logger.LogError(
                    ex,
                    "Error al modificar la asignación {AsignacionId}.",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo modificar la asignación. " +
                    "Vuelva al listado e intente nuevamente.");
            }

            await CargarRutinasParaCambioAsync(model, entrenadorId.Value);

            return View(model);
        }

        private async Task CargarRutinasParaCambioAsync(
            ModificarAsignacionViewModel model,
            int entrenadorId)
        {
            var asignacion = await _context.AsignacionRutinas
                .AsNoTracking()
                .Where(a =>
                    a.AsignacionId == model.AsignacionId &&
                    a.Estado == "ACTIVA" &&
                    a.Cliente.EntrenadorId == entrenadorId &&
                    a.Cliente.Usuario.Estado == "ACTIVO" &&
                    a.Cliente.Usuario.Rol.Nombre == "CLIENTE")
                .Select(a => new
                {
                    a.ClienteId,
                    NombreCliente = a.Cliente.Usuario.Nombre,
                    NombreRutina = a.Rutina.Nombre
                })
                .SingleOrDefaultAsync();

            model.RutinasDisponibles = new List<SelectListItem>();

            if (asignacion == null)
                return;

            model.NombreCliente = asignacion.NombreCliente;
            model.NombreRutinaActual = asignacion.NombreRutina;

            var rutinas = await _context.Rutinas
                .AsNoTracking()
                .Where(r =>
                    r.EntrenadorId == entrenadorId &&
                    r.Estado == "ACTIVA" &&
                    !_context.AsignacionRutinas.Any(a =>
                        a.ClienteId == asignacion.ClienteId &&
                        a.RutinaId == r.RutinaId &&
                        a.Estado == "ACTIVA"))
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.RutinaId,
                    r.Nombre
                })
                .ToListAsync();

            model.RutinasDisponibles = rutinas
                .Select(r => new SelectListItem
                {
                    Value = r.RutinaId.ToString(),
                    Text = r.Nombre
                })
                .ToList();
        }

        private async Task<int?> ObtenerEntrenadorActivoAsync()
        {
            var identificador =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificador, out var usuarioId))
            {
                return null;
            }

            var entrenadorValido = await _context.Entrenadors
                .AsNoTracking()
                .AnyAsync(e =>
                    e.UsuarioId == usuarioId &&
                    e.Usuario.Estado == "ACTIVO" &&
                    e.Usuario.Rol.Nombre == "ENTRENADOR");

            return entrenadorValido ? usuarioId : null;
        }

        private static string? LimpiarOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
    }
}
