using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class VisitantesController : Controller
    {
        public IActionResult Informacion()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        public IActionResult RegistroExitoso()
        {
            return View();
        }
    }
}