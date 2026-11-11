using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KRO_Training_Performance.Controllers
{
    // Todos los roles autenticados pueden ver los eventos.
    [Authorize]
    public class EventosController : Controller
    {
        private readonly KroTrainingContext _context;

        public EventosController(KroTrainingContext context)
        {
            _context = context;
        }

        // GET: /Eventos/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.PuedeGestionar = User.IsInRole("ADMINISTRADOR");

            try
            {
                var eventos = await _context.Eventos
                    .AsNoTracking()
                    .OrderBy(e => e.Fecha)
                    .Select(e => new EventoListaViewModel
                    {
                        EventoId = e.EventoId,
                        Nombre = e.Nombre,
                        Descripcion = e.Descripcion,
                        Fecha = e.Fecha,
                        CupoMaximo = e.CupoMaximo,
                        Inscritos = e.InscripcionEventos.Count(i => i.Estado != "CANCELADO"),
                        Estado = e.Estado
                    })
                    .ToListAsync();

                return View(eventos);
            }
            catch
            {
                ViewBag.ErrorCarga = "Error al cargar los eventos. Intente nuevamente más tarde.";
                return View(new List<EventoListaViewModel>());
            }
        }

        // GET: /Eventos/Crear (solo administrador)
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public IActionResult Crear()
        {
            return View(new EventoFormViewModel
            {
                Fecha = DateTime.Today.AddDays(7).AddHours(18)
            });
        }

        // POST: /Eventos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Crear(EventoFormViewModel model)
        {
            ValidarEvento(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int administradorId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            _context.Eventos.Add(new Evento
            {
                Nombre = model.Nombre.Trim(),
                Descripcion = LimpiarTexto(model.Descripcion),
                Fecha = model.Fecha!.Value,
                CupoMaximo = model.CupoMaximo,
                Estado = EstadosEvento.Activo,
                AdministradorId = administradorId
            });

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El evento fue creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Eventos/Editar/5 (solo administrador)
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Editar(int id)
        {
            var evento = await _context.Eventos
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventoId,
                    e.Nombre,
                    e.Descripcion,
                    e.Fecha,
                    e.CupoMaximo,
                    e.Estado,
                    Inscritos = e.InscripcionEventos.Count(i => i.Estado != "CANCELADO")
                })
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null)
            {
                return NotFound("El evento no fue encontrado.");
            }

            return View(new EventoFormViewModel
            {
                EventoId = evento.EventoId,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha,
                CupoMaximo = evento.CupoMaximo,
                Estado = evento.Estado,
                InscritosActuales = evento.Inscritos
            });
        }

        // POST: /Eventos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Editar(int id, EventoFormViewModel model)
        {
            if (model.EventoId != id)
            {
                return BadRequest("El identificador del evento no coincide.");
            }

            ValidarEvento(model);

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null)
            {
                return NotFound("El evento no fue encontrado.");
            }

            model.InscritosActuales = await _context.InscripcionEventos
                .CountAsync(i => i.EventoId == id && i.Estado != "CANCELADO");

            if (model.CupoMaximo < model.InscritosActuales)
            {
                ModelState.AddModelError(nameof(model.CupoMaximo),
                    $"El cupo no puede ser menor a los {model.InscritosActuales} inscritos actuales.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            evento.Nombre = model.Nombre.Trim();
            evento.Descripcion = LimpiarTexto(model.Descripcion);
            evento.Fecha = model.Fecha!.Value;
            evento.CupoMaximo = model.CupoMaximo;
            evento.Estado = model.Estado;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El evento se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidarEvento(EventoFormViewModel model)
        {
            if (!EstadosEvento.Todos.Contains(model.Estado))
            {
                ModelState.AddModelError(nameof(model.Estado), "Seleccione un estado válido.");
            }
        }

        private static string? LimpiarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
