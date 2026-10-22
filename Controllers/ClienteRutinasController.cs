
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KROTraining.Models;
using KROTraining.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KROTraining.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class ClienteRutinasController : Controller
    {
        private readonly KroTrainingContext _context;

        public ClienteRutinasController(KroTrainingContext context)
        {
            _context = context;
        }

        private async Task<int?> ObtenerClienteActivoAsync()
        {
            var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificador, out int usuarioId))
                return null;

            bool valido = await _context.Clientes
                .AsNoTracking()
                .AnyAsync(c =>
                    c.UsuarioId == usuarioId &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE");

            return valido ? usuarioId : null;
        }

        // HU-1: consultar la rutina asignada
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var asignacion = await _context.AsignacionRutinas
                .AsNoTracking()
                .Where(a =>
                    a.ClienteId == clienteId.Value &&
                    a.Estado == "ACTIVA")
                .OrderByDescending(a => a.FechaAsignacion)
                .ThenByDescending(a => a.AsignacionId)
                .Include(a => a.Rutina)
                    .ThenInclude(r => r.RutinaEjercicios)
                        .ThenInclude(re => re.Ejercicio)
                .FirstOrDefaultAsync();

            if (asignacion == null)
                return View(new ClienteRutinaViewModel());

            var sesionActiva = await _context.SesionEntrenamientos
                .Where(s =>
                    s.AsignacionId == asignacion.AsignacionId &&
                    s.Estado == "EN_PROGRESO")
                .OrderByDescending(s => s.Fecha)
                .ThenByDescending(s => s.SesionId)
                .FirstOrDefaultAsync();

            var model = new ClienteRutinaViewModel
            {
                AsignacionId = asignacion.AsignacionId,
                NombreRutina = asignacion.Rutina.Nombre,
                Descripcion = asignacion.Rutina.Descripcion,
                Objetivo = asignacion.Rutina.Objetivo,
                Nivel = asignacion.Rutina.Nivel,
                FechaAsignacion = asignacion.FechaAsignacion,
                SesionEnProgresoId = sesionActiva?.SesionId,
                Ejercicios = asignacion.Rutina.RutinaEjercicios
                    .OrderBy(re => re.Orden)
                    .Select(re => new ClienteRutinaEjercicioViewModel
                    {
                        EjercicioId = re.EjercicioId,
                        Nombre = re.Ejercicio.Nombre,
                        Descripcion = re.Ejercicio.Descripcion,
                        GrupoMuscular = re.Ejercicio.GrupoMuscular,
                        Series = re.Series,
                        Repeticiones = re.Repeticiones,
                        DescansoSegundos = re.DescansoSegundos,
                        Completado = false
                    })
                    .ToList()
            };

            if (sesionActiva != null)
            {
                var marcados = await _context.SesionEjercicios
                    .Where(se => se.SesionId == sesionActiva.SesionId)
                    .ToDictionaryAsync(se => se.EjercicioId, se => se.Completado);

                foreach (var ejercicio in model.Ejercicios)
                {
                    if (marcados.TryGetValue(ejercicio.EjercicioId, out bool completado))
                        ejercicio.Completado = completado;
                }
            }

            return View(model);
        }

        // HU-2: iniciar una sesión o continuar la que ya está en progreso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(int asignacionId)
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var asignacion = await _context.AsignacionRutinas
                .Include(a => a.Rutina)
                    .ThenInclude(r => r.RutinaEjercicios)
                .FirstOrDefaultAsync(a =>
                    a.AsignacionId == asignacionId &&
                    a.ClienteId == clienteId.Value &&
                    a.Estado == "ACTIVA");

            if (asignacion == null)
                return NotFound();

            var existente = await _context.SesionEntrenamientos
                .Where(s =>
                    s.AsignacionId == asignacionId &&
                    s.Estado == "EN_PROGRESO")
                .OrderByDescending(s => s.Fecha)
                .FirstOrDefaultAsync();

            if (existente != null)
                return RedirectToAction(nameof(Sesion), new { id = existente.SesionId });

            if (!asignacion.Rutina.RutinaEjercicios.Any())
            {
                TempData["Mensaje"] = "Esta rutina todavía no tiene ejercicios.";
                return RedirectToAction(nameof(Index));
            }

            var sesion = new SesionEntrenamiento
            {
                AsignacionId = asignacionId,
                Fecha = DateTime.Now,
                Estado = "EN_PROGRESO"
            };

            _context.SesionEntrenamientos.Add(sesion);
            await _context.SaveChangesAsync();

            foreach (var re in asignacion.Rutina.RutinaEjercicios)
            {
                _context.SesionEjercicios.Add(new SesionEjercicio
                {
                    SesionId = sesion.SesionId,
                    EjercicioId = re.EjercicioId,
                    Completado = false,
                    SeriesAsignadas = re.Series,
                    RepeticionesAsignadas = re.Repeticiones,
                    DescansoSegundosAsignado = re.DescansoSegundos
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Sesion), new { id = sesion.SesionId });
        }

        // HU-2: ver el estado actual de una sesión propia
        [HttpGet]
        public async Task<IActionResult> Sesion(int id)
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var sesion = await _context.SesionEntrenamientos
                .AsNoTracking()
                .Include(s => s.Asignacion)
                    .ThenInclude(a => a.Rutina)
                .Include(s => s.SesionEjercicios)
                    .ThenInclude(se => se.Ejercicio)
                .FirstOrDefaultAsync(s =>
                    s.SesionId == id &&
                    s.Asignacion.ClienteId == clienteId.Value);

            if (sesion == null)
                return NotFound();

            var model = new ClienteSesionViewModel
            {
                SesionId = sesion.SesionId,
                NombreRutina = sesion.Asignacion.Rutina.Nombre,
                Fecha = sesion.Fecha,
                Estado = sesion.Estado,
                TotalEjercicios = sesion.SesionEjercicios.Count,
                EjerciciosCompletados = sesion.SesionEjercicios.Count(e => e.Completado),
                Ejercicios = sesion.SesionEjercicios
                    .OrderBy(se => se.Ejercicio.Nombre)
                    .Select(se => new ClienteSesionEjercicioViewModel
                    {
                        EjercicioId = se.EjercicioId,
                        Nombre = se.Ejercicio.Nombre,
                        GrupoMuscular = se.Ejercicio.GrupoMuscular,
                        SeriesAsignadas = se.SeriesAsignadas,
                        RepeticionesAsignadas = se.RepeticionesAsignadas,
                        DescansoSegundosAsignado = se.DescansoSegundosAsignado,
                        Completado = se.Completado,
                        FechaMarcado = se.FechaMarcado
                    })
                    .ToList()
            };

            return View(model);
        }

        // HU-2: marcar o desmarcar un ejercicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEjercicio(
            int sesionId, int ejercicioId, bool completado)
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var sesion = await _context.SesionEntrenamientos
                .FirstOrDefaultAsync(s =>
                    s.SesionId == sesionId &&
                    s.Asignacion.ClienteId == clienteId.Value);

            if (sesion == null)
                return NotFound();

            if (sesion.Estado != "EN_PROGRESO")
            {
                TempData["Mensaje"] = "Esta sesión ya fue finalizada.";
                return RedirectToAction(nameof(Sesion), new { id = sesionId });
            }

            var registro = await _context.SesionEjercicios
                .FirstOrDefaultAsync(se =>
                    se.SesionId == sesionId &&
                    se.EjercicioId == ejercicioId);

            if (registro == null)
                return NotFound();

            registro.Completado = completado;
            registro.FechaMarcado = completado ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Sesion), new { id = sesionId });
        }

        // HU-2: finalizar la sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarSesion(int id)
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var sesion = await _context.SesionEntrenamientos
                .Include(s => s.SesionEjercicios)
                .FirstOrDefaultAsync(s =>
                    s.SesionId == id &&
                    s.Asignacion.ClienteId == clienteId.Value);

            if (sesion == null)
                return NotFound();

            if (sesion.Estado == "EN_PROGRESO")
            {
                sesion.Estado = "FINALIZADA";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }

        // HU-3: historial de sesiones anteriores
        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var sesiones = await _context.SesionEntrenamientos
                .AsNoTracking()
                .Where(s => s.Asignacion.ClienteId == clienteId.Value)
                .Include(s => s.Asignacion)
                    .ThenInclude(a => a.Rutina)
                .Include(s => s.SesionEjercicios)
                .OrderByDescending(s => s.Fecha)
                .ThenByDescending(s => s.SesionId)
                .Select(s => new ClienteHistorialItemViewModel
                {
                    SesionId = s.SesionId,
                    NombreRutina = s.Asignacion.Rutina.Nombre,
                    Fecha = s.Fecha,
                    Estado = s.Estado,
                    TotalEjercicios = s.SesionEjercicios.Count,
                    EjerciciosCompletados =
                        s.SesionEjercicios.Count(e => e.Completado)
                })
                .ToListAsync();

            return View(sesiones);
        }

        // HU-3: detalle de una sesión histórica propia
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            int? clienteId = await ObtenerClienteActivoAsync();

            if (clienteId == null)
                return Forbid();

            var sesion = await _context.SesionEntrenamientos
                .AsNoTracking()
                .Include(s => s.Asignacion)
                    .ThenInclude(a => a.Rutina)
                .Include(s => s.SesionEjercicios)
                    .ThenInclude(se => se.Ejercicio)
                .FirstOrDefaultAsync(s =>
                    s.SesionId == id &&
                    s.Asignacion.ClienteId == clienteId.Value);

            if (sesion == null)
                return NotFound();

            var model = new ClienteSesionViewModel
            {
                SesionId = sesion.SesionId,
                NombreRutina = sesion.Asignacion.Rutina.Nombre,
                Fecha = sesion.Fecha,
                Estado = sesion.Estado,
                TotalEjercicios = sesion.SesionEjercicios.Count,
                EjerciciosCompletados =
                    sesion.SesionEjercicios.Count(e => e.Completado),
                Ejercicios = sesion.SesionEjercicios
                    .OrderBy(se => se.Ejercicio.Nombre)
                    .Select(se => new ClienteSesionEjercicioViewModel
                    {
                        EjercicioId = se.EjercicioId,
                        Nombre = se.Ejercicio.Nombre,
                        GrupoMuscular = se.Ejercicio.GrupoMuscular,
                        SeriesAsignadas = se.SeriesAsignadas,
                        RepeticionesAsignadas = se.RepeticionesAsignadas,
                        DescansoSegundosAsignado = se.DescansoSegundosAsignado,
                        Completado = se.Completado,
                        FechaMarcado = se.FechaMarcado
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}
