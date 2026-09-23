using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Desercion()
        {
            return View();
        }

        public IActionResult Estadisticas()
        {
            return View();
        }

        public IActionResult Exportar()
        {
            return View();
        }
    }
}