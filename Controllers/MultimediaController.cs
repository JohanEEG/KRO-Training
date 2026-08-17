using Microsoft.AspNetCore.Mvc;

namespace KroTraining.Controllers
{
    public class MultimediaController : Controller
    {
        public IActionResult CrearEjercicio()
        {
            return View();
        }

        public IActionResult AdjuntarMultimedia()
        {
            return View();
        }

        public IActionResult VerMultimedia()
        {
            return View();
        }
    }
}