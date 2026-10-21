using System.Data;
using System.Security.Claims;
using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
