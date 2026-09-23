using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class MembresiasController : Controller
    {
        public IActionResult CrearPlan()
        {
            return View("~/Views/MembresiasPagos/CrearPlan.cshtml");
        }

        public IActionResult ProximasVencer()
        {
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