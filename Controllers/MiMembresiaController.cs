using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KROTraining.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class MiMembresiaController : Controller
    {
        private readonly KroTrainingContext _context;

        public MiMembresiaController(KroTrainingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var estado = await MembresiaHelper.ObtenerEstadoAsync(_context, usuarioId);

            var pendientes = await _context.Notificacions
                .Where(n => n.UsuarioId == usuarioId && n.Estado == "NO_LEIDA")
                .OrderByDescending(n => n.Fecha)
                .ToListAsync();

            ViewBag.Notificaciones = pendientes.ToList();

            foreach (var n in pendientes) n.Estado = "LEIDA";
            if (pendientes.Any()) await _context.SaveChangesAsync();

            return View(estado);
        }

        // GET: /MiMembresia/Historial?fechaInicio=yyyy-MM-dd&fechaFin=yyyy-MM-dd
        [HttpGet]
        public async Task<IActionResult> Historial(DateOnly? fechaInicio, DateOnly? fechaFin)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                ViewBag.Error = "La fecha inicial no puede ser posterior a la fecha final.";
                return View(new List<Membresium>());
            }

            try
            {
                var consulta = _context.Membresia
                    .AsNoTracking()
                    .Include(m => m.Plan)
                    .Include(m => m.Pagos)
                    .Where(m => m.ClienteId == usuarioId);

                if (fechaInicio.HasValue)
                {
                    consulta = consulta.Where(m => m.FechaInicio >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    consulta = consulta.Where(m => m.FechaInicio <= fechaFin.Value);
                }

                var historial = await consulta
                    .OrderByDescending(m => m.FechaInicio)
                    .ToListAsync();

                return View(historial);
            }
            catch
            {
                ViewBag.Error = "Error al cargar el historial de membresías. Intente nuevamente más tarde.";
                return View(new List<Membresium>());
            }
        }
    }
}