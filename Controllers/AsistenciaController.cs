using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
using System.Security.Claims;

namespace KROTraining.Controllers
{
    [Authorize]
    public class ControlAsistenciaController : Controller
    {
        private readonly KroTrainingContext _context;

        public ControlAsistenciaController(KroTrainingContext context)
        {
            _context = context;
        }

        // HU-55 — Como cliente quiero registrar mi ingreso al gimnasio
        [Authorize(Roles = "CLIENTE")]
        [HttpGet]
        public async Task<IActionResult> RegistrarIngreso()
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Membresia)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            return View(cliente);
        }

        [Authorize(Roles = "CLIENTE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarIngreso(IFormCollection form)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // Criterio: membresía vencida
            var membresiaActiva = await _context.Membresia
                .AnyAsync(m => m.ClienteId == usuarioId
                            && m.Estado == "ACTIVA"
                            && m.FechaFin >= hoy);

            if (!membresiaActiva)
            {
                TempData["Error"] = "Tu membresía está vencida. Renuévala para registrar tu ingreso.";
                return RedirectToAction(nameof(RegistrarIngreso));
            }

            // Criterio: ingreso ya registrado
            var yaRegistrado = await _context.Asistencia
                .AnyAsync(a => a.ClienteId == usuarioId && a.Fecha == hoy);

            if (yaRegistrado)
            {
                TempData["Error"] = "Ya registraste tu ingreso hoy.";
                return RedirectToAction(nameof(RegistrarIngreso));
            }

            // Criterio: registro de ingreso exitoso
            _context.Asistencia.Add(new Asistencium
            {
                ClienteId = usuarioId,
                Fecha = hoy,
                Hora = TimeOnly.FromDateTime(DateTime.Now)
            });
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "¡Ha registrado su asistencia de hoy!";
            return RedirectToAction(nameof(RegistrarIngreso));
        }

        // HU-56 — Como administrador quiero consultar el historial de asistencia
        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> HistorialAsistencia(DateOnly? fechaInicio, DateOnly? fechaFin)
        {
            var query = _context.Asistencia
                .Include(a => a.Cliente).ThenInclude(c => c.Usuario)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.Fecha <= fechaFin.Value);

            var resultados = await query.OrderByDescending(a => a.Fecha).ToListAsync();

            ViewBag.SinResultados = !resultados.Any(); // Criterio: sin registros de asistencia
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(resultados);
        }

        // HU-57 — Como entrenador quiero visualizar la asistencia de mis clientes asignados
        [Authorize(Roles = "ENTRENADOR")]
        [HttpGet]
        public async Task<IActionResult> VisualizarAsistencia()
        {
            try
            {
                int entrenadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var misClientes = await _context.Clientes
                    .Include(c => c.Usuario)
                    .Include(c => c.Asistencia)
                    .Where(c => c.EntrenadorId == entrenadorId)
                    .ToListAsync();

                return View(misClientes);
            }
            catch
            {
                // Criterio: error al cargar la información
                return View("Error");
            }
        }
    }
}