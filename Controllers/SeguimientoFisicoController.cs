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