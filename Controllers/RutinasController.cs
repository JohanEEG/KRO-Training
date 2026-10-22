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
