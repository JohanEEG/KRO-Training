using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KRO_Training_Performance.Controllers
{
    [Authorize]
    public class EventosController : Controller
    {
        private readonly KroTrainingContext _context;

        public EventosController(KroTrainingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.PuedeGestionar = User.IsInRole("ADMINISTRADOR");
            ViewBag.PuedeGestionarInscripciones = User.IsInRole("ADMINISTRADOR") || User.IsInRole("ENTRENADOR");

            try
            {
                var esCliente = User.IsInRole("CLIENTE");
                int? clienteId = null;
                if (esCliente && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
                {
                    clienteId = usuarioId;
                }

                var consulta = _context.Eventos.AsNoTracking();
                if (esCliente && clienteId.HasValue)
                {
                    var ahora = DateTime.Now;
                    consulta = consulta.Where(e =>
                        (e.Estado == EstadosEvento.Activo && e.Fecha > ahora) ||
                        e.InscripcionEventos.Any(i =>
                            i.ClienteId == clienteId.Value && i.Estado != "CANCELADO"));
                }

                var eventos = await consulta
                    .OrderBy(e => e.Fecha)
                    .Select(e => new EventoListaViewModel
                    {
                        EventoId = e.EventoId,
                        Nombre = e.Nombre,
                        Descripcion = e.Descripcion,
                        Fecha = e.Fecha,
                        CupoMaximo = e.CupoMaximo,
                        Inscritos = e.InscripcionEventos.Count(i => i.Estado != "CANCELADO"),
                        Estado = e.Estado,
                        EstaInscrito = clienteId.HasValue && e.InscripcionEventos.Any(i =>
                            i.ClienteId == clienteId.Value && i.Estado != "CANCELADO")
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

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var evento = await _context.Eventos
                .AsNoTracking()
                .Select(e => new EventoDetalleViewModel
                {
                    EventoId = e.EventoId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Fecha = e.Fecha,
                    CupoMaximo = e.CupoMaximo,
                    Inscritos = e.InscripcionEventos.Count(i => i.Estado != "CANCELADO"),
                    Estado = e.Estado
                })
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null)
            {
                return NotFound("El evento no fue encontrado.");
            }

            evento.PuedeGestionar = User.IsInRole("ADMINISTRADOR");
            evento.PuedeGestionarInscripciones = User.IsInRole("ADMINISTRADOR") || User.IsInRole("ENTRENADOR");
            evento.EsCliente = User.IsInRole("CLIENTE");

            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            {
                evento.YaInscrito = await _context.InscripcionEventos.AnyAsync(i =>
                    i.EventoId == id && i.ClienteId == usuarioId && i.Estado != "CANCELADO");
            }

            evento.PuedeInscribirse = evento.EsCliente &&
                evento.Estado == EstadosEvento.Activo &&
                evento.Fecha > DateTime.Now &&
                evento.CuposDisponibles > 0 &&
                !evento.YaInscrito;

            return View(evento);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public IActionResult Crear()
        {
            return View(new EventoFormViewModel
            {
                Fecha = DateTime.Today.AddDays(7).AddHours(18)
            });
        }

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

            var nombre = model.Nombre.Trim();
            var descripcion = LimpiarTexto(model.Descripcion);
            var sinCambios = evento.Nombre == nombre &&
                evento.Descripcion == descripcion &&
                evento.Fecha == model.Fecha!.Value &&
                evento.CupoMaximo == model.CupoMaximo &&
                evento.Estado == model.Estado;

            if (sinCambios)
            {
                TempData["MensajeExito"] = "No se detectaron cambios en el evento.";
                return RedirectToAction(nameof(Index));
            }

            evento.Nombre = nombre;
            evento.Descripcion = descripcion;
            evento.Fecha = model.Fecha!.Value;
            evento.CupoMaximo = model.CupoMaximo;
            evento.Estado = model.Estado;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El evento se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.EventoId == id);
            if (evento == null)
            {
                return NotFound("El evento no fue encontrado.");
            }

            if (evento.Estado == EstadosEvento.Cancelado)
            {
                TempData["MensajeError"] = "El evento ya se encuentra cancelado.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            evento.Estado = EstadosEvento.Cancelado;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El evento fue cancelado correctamente. Las inscripciones existentes se conservaron.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        public async Task<IActionResult> Inscripciones(int? id)
        {
            var eventos = await ObtenerEventosAsync();
            var eventoId = id ?? eventos.FirstOrDefault()?.EventoId;

            if (!eventoId.HasValue)
            {
                return View(new InscripcionesViewModel { Eventos = eventos });
            }

            var modelo = await ConstruirInscripcionesAsync(eventoId.Value, eventos);
            if (modelo == null)
            {
                return NotFound("El evento no fue encontrado.");
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> Inscribir(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clienteId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var resultado = await RegistrarInscripcionAsync(id, clienteId);
            if (resultado != null)
            {
                TempData["MensajeError"] = resultado;
                return RedirectToAction(nameof(Detalle), new { id });
            }

            TempData["MensajeExito"] = "La inscripción se registró correctamente.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        public async Task<IActionResult> InscribirCliente(int eventoId, int clienteId)
        {
            var resultado = await RegistrarInscripcionAsync(eventoId, clienteId);
            if (resultado != null)
            {
                TempData["MensajeError"] = resultado;
            }
            else
            {
                TempData["MensajeExito"] = "La inscripción se registró correctamente.";
            }

            return RedirectToAction(nameof(Inscripciones), new { id = eventoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelarInscripcion(int eventoId, int? clienteId)
        {
            var puedeGestionarInscripciones = User.IsInRole("ADMINISTRADOR") || User.IsInRole("ENTRENADOR");
            if (!puedeGestionarInscripciones)
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
                {
                    return RedirectToAction("Login", "Cuenta");
                }

                clienteId = usuarioId;
            }

            if (!clienteId.HasValue)
            {
                return BadRequest("Debe indicar el cliente.");
            }

            var inscripcion = await _context.InscripcionEventos
                .FirstOrDefaultAsync(i => i.EventoId == eventoId && i.ClienteId == clienteId.Value);

            if (inscripcion == null)
            {
                return NotFound("La inscripción no fue encontrada.");
            }

            inscripcion.Estado = "CANCELADO";
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La inscripción fue cancelada correctamente.";
            return puedeGestionarInscripciones
                ? RedirectToAction(nameof(Inscripciones), new { id = eventoId })
                : RedirectToAction(nameof(Detalle), new { id = eventoId });
        }

        private async Task<string?> RegistrarInscripcionAsync(int eventoId, int clienteId)
        {
            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.EventoId == eventoId);

            if (evento == null)
            {
                return "El evento no fue encontrado.";
            }

            if (evento.Estado != EstadosEvento.Activo || evento.Fecha <= DateTime.Now)
            {
                return "El evento no está disponible para nuevas inscripciones.";
            }

            if (!await _context.Clientes.AnyAsync(c => c.UsuarioId == clienteId))
            {
                return "El cliente no fue encontrado.";
            }

            var inscripcion = await _context.InscripcionEventos
                .FirstOrDefaultAsync(i => i.EventoId == eventoId && i.ClienteId == clienteId);

            if (inscripcion != null && inscripcion.Estado != "CANCELADO")
            {
                return "El cliente ya se encuentra inscrito en este evento.";
            }

            var inscritos = await _context.InscripcionEventos
                .CountAsync(i => i.EventoId == eventoId && i.Estado != "CANCELADO");

            if (inscritos >= evento.CupoMaximo)
            {
                return "El evento no tiene cupos disponibles.";
            }

            if (inscripcion == null)
            {
                _context.InscripcionEventos.Add(new InscripcionEvento
                {
                    EventoId = eventoId,
                    ClienteId = clienteId,
                    Estado = "INSCRITO",
                    FechaInscripcion = DateTime.Now
                });
            }
            else
            {
                inscripcion.Estado = "INSCRITO";
                inscripcion.FechaInscripcion = DateTime.Now;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return "No fue posible registrar la inscripción. Verifique si el cliente ya fue inscrito.";
            }

            return null;
        }

        private async Task<List<EventoListaViewModel>> ObtenerEventosAsync()
        {
            return await _context.Eventos
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
        }

        private async Task<InscripcionesViewModel?> ConstruirInscripcionesAsync(
            int eventoId,
            List<EventoListaViewModel> eventos)
        {
            var evento = await _context.Eventos
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventoId == eventoId);

            if (evento == null)
            {
                return null;
            }

            var inscripciones = await _context.InscripcionEventos
                .AsNoTracking()
                .Where(i => i.EventoId == eventoId)
                .OrderBy(i => i.Cliente.Usuario.Nombre)
                .Select(i => new InscripcionEventoListaViewModel
                {
                    EventoId = i.EventoId,
                    ClienteId = i.ClienteId,
                    ClienteNombre = i.Cliente.Usuario.Nombre,
                    ClienteCorreo = i.Cliente.Usuario.Correo,
                    FechaInscripcion = i.FechaInscripcion,
                    Estado = i.Estado
                })
                .ToListAsync();

            var inscritosIds = inscripciones
                .Where(i => i.Estado != "CANCELADO")
                .Select(i => i.ClienteId)
                .ToHashSet();

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => !inscritosIds.Contains(c.UsuarioId) && c.Usuario.Estado == "ACTIVO")
                .OrderBy(c => c.Usuario.Nombre)
                .Select(c => new ClienteEventoViewModel
                {
                    ClienteId = c.UsuarioId,
                    Nombre = c.Usuario.Nombre,
                    Correo = c.Usuario.Correo
                })
                .ToListAsync();

            return new InscripcionesViewModel
            {
                EventoId = eventoId,
                EventoNombre = evento.Nombre,
                CupoMaximo = evento.CupoMaximo,
                Inscritos = inscritosIds.Count,
                Eventos = eventos,
                Inscripciones = inscripciones,
                ClientesDisponibles = clientes
            };
        }

        private void ValidarEvento(EventoFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError(nameof(model.Nombre), "El nombre del evento es obligatorio.");
            }

            if (!model.Fecha.HasValue || model.Fecha.Value <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.Fecha), "La fecha del evento debe ser posterior a la fecha actual.");
            }

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
