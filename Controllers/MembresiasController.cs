using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRO_Training_Performance.Controllers
{
    public class MembresiasController : Controller
    {
        private readonly KroTrainingContext _context;

        public MembresiasController(KroTrainingContext context)
        {
            _context = context;
        }

        // HU-69: la creación de planes vive en la pantalla de Próximas a vencer
        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public IActionResult CrearPlan()
        {
            return RedirectToAction(nameof(ProximasVencer));
        }

        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPlan(string? nombre, int? duracionMeses, decimal? precio)
        {
            nombre = nombre?.Trim();

            // Criterio: campos obligatorios incompletos
            if (string.IsNullOrEmpty(nombre) || duracionMeses == null || precio == null)
            {
                return VolverConError("Complete todos los campos obligatorios.", nombre, duracionMeses, precio);
            }
            if (duracionMeses <= 0)
            {
                return VolverConError("La duración debe ser de al menos 1 mes.", nombre, duracionMeses, precio);
            }
            if (precio <= 0)
            {
                return VolverConError("El precio debe ser mayor a 0.", nombre, duracionMeses, precio);
            }

            // Criterio: nombre de plan duplicado (sin distinguir mayúsculas)
            bool existe = await _context.PlanMembresia
                .AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower());
            if (existe)
            {
                return VolverConError($"Ya existe un plan llamado \"{nombre}\".", nombre, duracionMeses, precio);
            }

            // Criterio: creación exitosa
            _context.PlanMembresia.Add(new PlanMembresium
            {
                Nombre = nombre,
                DuracionMeses = duracionMeses.Value,
                Precio = precio.Value
            });
            await _context.SaveChangesAsync();

            TempData["PlanExito"] = $"El plan \"{nombre}\" se creó correctamente.";
            return RedirectToAction(nameof(ProximasVencer));
        }

        private IActionResult VolverConError(string mensaje, string? nombre, int? duracion, decimal? precio)
        {
            TempData["PlanError"] = mensaje;
            TempData["PlanNombre"] = nombre;
            TempData["PlanDuracion"] = duracion?.ToString();
            TempData["PlanPrecio"] = precio?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            return RedirectToAction(nameof(ProximasVencer));
        }

        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> ProximasVencer()
        {
            ViewBag.Planes = await _context.PlanMembresia
                .OrderBy(p => p.DuracionMeses)
                .ToListAsync();

            return View("~/Views/MembresiasPagos/ProximasVencer.cshtml");
        }

        public IActionResult RegistrarPago()
        {
            return View("~/Views/MembresiasPagos/RegistrarPago.cshtml");
        }

        public IActionResult ReportePagos()
        {
            return View("~/Views/MembresiasPagos/ReportePagos.cshtml");
        }
    }
}