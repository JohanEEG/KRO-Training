using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class SeguimientoFisicoController : Controller
    {
        public IActionResult ProgresoFisico()
        {
            return View();
        }

        public IActionResult RegistrarEvaluacion()
        {
            return View();
        }

        public IActionResult HistorialEvaluaciones()
        {
            return View();
        }
    }
}