using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        // HU-56 — Como administrador / entrenador quiero consultar el historial de asistencia con desplegable y filtros
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        [HttpGet]
        public async Task<IActionResult> HistorialAsistencia(int? clienteId, string? buscarCliente, DateOnly? fechaInicio, DateOnly? fechaFin, int pagina = 1)
        {
            int registrosPorPagina = 10;

            // 1. Cargar lista completa de clientes para el desplegable (<select>)
            var listaClientes = await _context.Clientes
                .Include(c => c.Usuario)
                .Where(c => c.Usuario != null)
                .OrderBy(c => c.Usuario!.Nombre)
                .Select(c => new
                {
                    ClienteId = c.UsuarioId,
                    NombreCompleto = $"{c.Usuario!.Nombre} (Badge: {c.BadgeNumero ?? "N/A"}) - Céd: {c.Usuario.Cedula ?? "N/A"}"
                })
                .ToListAsync();

            ViewBag.ClientesList = listaClientes;

            var query = _context.Asistencia
                .Include(a => a.Cliente)
                    .ThenInclude(c => c.Usuario)
                .AsQueryable();

            // 2. Filtro por Cliente seleccionado en el desplegable (<select>)
            if (clienteId.HasValue && clienteId.Value > 0)
            {
                query = query.Where(a => a.ClienteId == clienteId.Value);
            }
            // 3. Filtro por Búsqueda Manual (Nombre, Cédula o N° Badge)
            else if (!string.IsNullOrEmpty(buscarCliente))
            {
                query = query.Where(a =>
                    (a.Cliente != null && a.Cliente.Usuario != null &&
                     (a.Cliente.Usuario.Nombre.Contains(buscarCliente) ||
                      (a.Cliente.Usuario.Cedula != null && a.Cliente.Usuario.Cedula.Contains(buscarCliente)))) ||
                    (a.Cliente != null && a.Cliente.BadgeNumero != null && a.Cliente.BadgeNumero.Contains(buscarCliente)));
            }

            // 4. Filtro por Rango de Fechas
            if (fechaInicio.HasValue)
                query = query.Where(a => a.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.Fecha <= fechaFin.Value);

            // 5. Paginación y Ordenamiento
            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var resultados = await query
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.Hora)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            ViewBag.SinResultados = !resultados.Any();
            ViewBag.TotalPaginas = totalPaginas > 0 ? totalPaginas : 1;
            ViewBag.PaginaActual = pagina;
            ViewBag.ClienteIdSeleccionado = clienteId;
            ViewBag.BuscarCliente = buscarCliente;
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