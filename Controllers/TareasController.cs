using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KROTraining.Controllers
{
    // Solo el administrador asigna y elimina tareas del personal.
    [Authorize(Roles = "ADMINISTRADOR")]
    public class TareasController : Controller
    {
        private static readonly string[] RolesPersonal =
        {
            "ADMINISTRADOR",
            "ENTRENADOR",
            "EMPLEADO"
        };

        private readonly KroTrainingContext _context;

        public TareasController(KroTrainingContext context)
        {
            _context = context;
        }

        // GET: /Tareas/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                await CargarPersonalAsync();

                var tareas = await _context.HorarioTareas
                    .AsNoTracking()
                    .Include(t => t.Usuario)
                    .OrderByDescending(t => t.Fecha)
                    .ThenBy(t => t.Usuario.Nombre)
                    .Take(300)
                    .ToListAsync();

                return View(tareas);
            }
            catch
            {
                ViewBag.ErrorCarga = "Error al cargar las tareas. Intente nuevamente más tarde.";
                return View(new List<HorarioTarea>());
            }
        }

        // POST: /Tareas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int usuarioId, DateOnly? fecha, string? tarea)
        {
            tarea = (tarea ?? string.Empty).Trim();

            if (usuarioId <= 0 || !fecha.HasValue)
            {
                TempData["Error"] = "Seleccione a la persona y la fecha de la tarea.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(tarea))
            {
                TempData["Error"] = "La descripción de la tarea es obligatoria.";
                return RedirectToAction(nameof(Index));
            }

            if (tarea.Length > 255)
            {
                TempData["Error"] = "La tarea no puede superar los 255 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool esPersonalActivo = await _context.Usuarios
                    .AnyAsync(u => u.UsuarioId == usuarioId &&
                                   u.Estado == "ACTIVO" &&
                                   RolesPersonal.Contains(u.Rol.Nombre));

                if (!esPersonalActivo)
                {
                    TempData["Error"] = "Solo se pueden asignar tareas a personal activo.";
                    return RedirectToAction(nameof(Index));
                }

                _context.HorarioTareas.Add(new HorarioTarea
                {
                    UsuarioId = usuarioId,
                    Fecha = fecha.Value,
                    Tarea = tarea,
                    Estado = "PENDIENTE"
                });

                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "La tarea fue asignada correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo asignar la tarea. Intente nuevamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Tareas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminadas = await _context.HorarioTareas
                .Where(t => t.HorarioId == id)
                .ExecuteDeleteAsync();

            TempData[eliminadas > 0 ? "MensajeExito" : "Error"] = eliminadas > 0
                ? "La tarea fue eliminada."
                : "La tarea no existe o ya fue eliminada.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarPersonalAsync()
        {
            var personal = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Where(u => u.Estado == "ACTIVO" && RolesPersonal.Contains(u.Rol.Nombre))
                .OrderBy(u => u.Nombre)
                .Select(u => new { u.UsuarioId, u.Nombre, Rol = u.Rol.Nombre })
                .ToListAsync();

            ViewBag.Personal = personal
                .Select(p => new SelectListItem($"{p.Nombre} ({p.Rol})", p.UsuarioId.ToString()))
                .ToList();
        }
    }
}
