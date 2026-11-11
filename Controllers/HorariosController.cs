using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KROTraining.Controllers
{
    public class HorariosController : Controller
    {
        // Personal que aparece en la cuadrícula de horarios.
        private static readonly string[] RolesPersonal =
        {
            "ADMINISTRADOR",
            "ENTRENADOR",
            "EMPLEADO"
        };

        private readonly KroTrainingContext _context;

        public HorariosController(KroTrainingContext context)
        {
            _context = context;
        }

        // GET: /Horarios/Index
        // Visible para administrador, entrenador y empleado (solo lectura salvo administrador).
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR,EMPLEADO")]
        public async Task<IActionResult> Index()
        {
            var vm = new HorarioPersonalViewModel
            {
                PuedeEditar = User.IsInRole("ADMINISTRADOR")
            };

            try
            {
                var personal = await _context.Usuarios
                    .AsNoTracking()
                    .Include(u => u.Rol)
                    .Where(u => u.Estado == "ACTIVO" && RolesPersonal.Contains(u.Rol.Nombre))
                    .OrderBy(u => u.Rol.Nombre)
                    .ThenBy(u => u.Nombre)
                    .ToListAsync();

                var horarios = await _context.HorarioPersonals
                    .AsNoTracking()
                    .ToListAsync();

                vm.Filas = personal
                    .Select(u =>
                    {
                        var fila = new FilaHorarioViewModel
                        {
                            UsuarioId = u.UsuarioId,
                            Nombre = u.Nombre,
                            Rol = u.Rol.Nombre
                        };

                        foreach (var h in horarios.Where(h => h.UsuarioId == u.UsuarioId))
                        {
                            fila.Dias[h.DiaSemana - 1] = h.Turno;
                        }

                        return fila;
                    })
                    .ToList();
            }
            catch
            {
                ViewBag.ErrorCarga = "Error al cargar la información del horario. Intente nuevamente más tarde.";
            }

            return View(vm);
        }

        // POST: /Horarios/Guardar
        // Solo administrador. Reemplaza el horario completo de las filas enviadas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Guardar(HorarioPersonalViewModel model)
        {
            var filas = model.Filas ?? new List<FilaHorarioViewModel>();

            if (filas.Count == 0)
            {
                TempData["Error"] = "No hay horarios para guardar.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var fila in filas)
            {
                foreach (var turno in fila.Dias)
                {
                    if (turno != null && turno.Trim().Length > HorarioPersonalViewModel.MaxLongitudTurno)
                    {
                        TempData["Error"] = $"Cada celda admite máximo {HorarioPersonalViewModel.MaxLongitudTurno} caracteres.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            var ids = filas.Select(f => f.UsuarioId).Distinct().ToList();

            try
            {
                // Solo se aceptan personas activas con rol de personal.
                var idsValidos = await _context.Usuarios
                    .AsNoTracking()
                    .Where(u => ids.Contains(u.UsuarioId) &&
                                u.Estado == "ACTIVO" &&
                                RolesPersonal.Contains(u.Rol.Nombre))
                    .Select(u => u.UsuarioId)
                    .ToListAsync();

                if (idsValidos.Count != ids.Count)
                {
                    TempData["Error"] = "Hay personal que no puede recibir horario. Recargue la página e intente de nuevo.";
                    return RedirectToAction(nameof(Index));
                }

                await using var transaccion = await _context.Database.BeginTransactionAsync();

                await _context.HorarioPersonals
                    .Where(h => ids.Contains(h.UsuarioId))
                    .ExecuteDeleteAsync();

                var nuevos = new List<HorarioPersonal>();

                foreach (var fila in filas.GroupBy(f => f.UsuarioId).Select(g => g.First()))
                {
                    for (int dia = 0; dia < 7; dia++)
                    {
                        var turno = fila.Dias[dia]?.Trim();

                        if (!string.IsNullOrEmpty(turno))
                        {
                            nuevos.Add(new HorarioPersonal
                            {
                                UsuarioId = fila.UsuarioId,
                                DiaSemana = (byte)(dia + 1),
                                Turno = turno
                            });
                        }
                    }
                }

                _context.HorarioPersonals.AddRange(nuevos);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] = "El horario se guardó correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo guardar el horario. Intente nuevamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Horarios/CambiarEstadoTarea
        // Solo el empleado dueño de la tarea: alterna entre PENDIENTE y REALIZADA.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EMPLEADO")]
        public async Task<IActionResult> CambiarEstadoTarea(int id, string? origen)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            try
            {
                var tarea = await _context.HorarioTareas
                    .FirstOrDefaultAsync(t => t.HorarioId == id && t.UsuarioId == usuarioId);

                if (tarea == null)
                {
                    TempData["Error"] = "La tarea no existe o no está asignada a usted.";
                }
                else
                {
                    tarea.Estado = tarea.Estado == EstadosTarea.Realizada
                        ? EstadosTarea.Pendiente
                        : EstadosTarea.Realizada;

                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = tarea.Estado == EstadosTarea.Realizada
                        ? "La tarea se marcó como realizada."
                        : "La tarea se marcó como pendiente.";
                }
            }
            catch
            {
                TempData["Error"] = "No se pudo actualizar la tarea. Intente nuevamente.";
            }

            return origen == "inicio"
                ? RedirectToAction("Index", "Home")
                : RedirectToAction(nameof(MisTareas));
        }

        // GET: /Horarios/MisTareas
        // Solo empleado: tareas asignadas desde hoy en adelante.
        [HttpGet]
        [Authorize(Roles = "EMPLEADO")]
        public async Task<IActionResult> MisTareas()
        {
            var tareas = new List<HorarioTarea>();

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            try
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);

                tareas = await _context.HorarioTareas
                    .AsNoTracking()
                    .Where(t => t.UsuarioId == usuarioId && t.Fecha >= hoy)
                    .OrderBy(t => t.Fecha)
                    .ToListAsync();
            }
            catch
            {
                ViewBag.ErrorCarga = "Error al cargar la información de tareas. Intente nuevamente más tarde.";
            }

            return View(tareas);
        }
    }
}
