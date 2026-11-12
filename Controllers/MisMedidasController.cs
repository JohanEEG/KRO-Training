using System.Linq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KROTraining.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class MisMedidasController : Controller
    {
        private readonly KroTrainingContext _context;
        private readonly ILogger<MisMedidasController> _logger;

        public MisMedidasController(
            KroTrainingContext context,
            ILogger<MisMedidasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // HU-17: Abrir formulario de medidas corporales.
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var clienteId = await ObtenerClienteActivoAsync();

            if (!clienteId.HasValue)
                return Forbid();

            return View(new RegistrarMedidasViewModel());
        }

        // HU-17: Guardar las medidas del cliente autenticado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarMedidasViewModel model)
        {
            var clienteId = await ObtenerClienteActivoAsync();

            if (!clienteId.HasValue)
                return Forbid();

            ValidarDecimales(model.Peso, nameof(model.Peso));
            ValidarDecimales(model.Estatura, nameof(model.Estatura));
            ValidarDecimales(model.Cintura, nameof(model.Cintura));
            ValidarDecimales(model.Cadera, nameof(model.Cadera));
            ValidarDecimales(model.Brazo, nameof(model.Brazo));

            if (!ModelState.IsValid)
                return View(model);

            var medida = new MedidaCorporal
            {
                ClienteId = clienteId.Value,
                Fecha = DateTime.Now,
                Peso = model.Peso!.Value,
                Estatura = model.Estatura!.Value,
                Cintura = model.Cintura,
                Cadera = model.Cadera,
                Brazo = model.Brazo
            };

            try
            {
                _context.MedidasCorporales.Add(medida);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "Sus medidas corporales se registraron correctamente.";

                return RedirectToAction(nameof(Registrar));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Error al guardar medidas del cliente {ClienteId}.",
                    clienteId.Value);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron guardar las medidas. Intente nuevamente.");

                return View(model);
            }
        }

        // HU-18: Consultar la evolución de las medidas del cliente.
        [HttpGet]
        public async Task<IActionResult> Evolucion()
        {
            var clienteId = await ObtenerClienteActivoAsync();

            if (!clienteId.HasValue)
                return Forbid();

            var registros = await _context.MedidasCorporales
                .AsNoTracking()
                .Where(m => m.ClienteId == clienteId.Value)
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.MedidaId)
                .Select(m => new PuntoEvolucionMedidasViewModel
                {
                    Fecha = m.Fecha,
                    Peso = m.Peso,
                    Estatura = m.Estatura,
                    Cintura = m.Cintura,
                    Cadera = m.Cadera,
                    Brazo = m.Brazo
                })
                .ToListAsync();

            var model = new EvolucionMedidasViewModel
            {
                Registros = registros
            };

            return View(model);
        }

        private async Task<int?> ObtenerClienteActivoAsync()
        {
            var identificador =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificador, out var usuarioId))
                return null;

            var clienteValido = await _context.Clientes
                .AsNoTracking()
                .AnyAsync(c =>
                    c.UsuarioId == usuarioId &&
                    c.Usuario.Estado == "ACTIVO" &&
                    c.Usuario.Rol.Nombre == "CLIENTE");

            return clienteValido ? usuarioId : null;
        }

        private void ValidarDecimales(decimal? valor, string campo)
        {
            if (valor.HasValue &&
                decimal.Round(valor.Value, 2) != valor.Value)
            {
                ModelState.AddModelError(
                    campo,
                    "Ingrese como máximo dos decimales.");
            }
        }
    }
}