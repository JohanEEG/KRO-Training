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
    }
}