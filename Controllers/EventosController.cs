using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class EventosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Editar()
        {
            return View();
        }
    }
}