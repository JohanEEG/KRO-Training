using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class ControlAsistenciaController : Controller
    {
        public IActionResult HistorialAsistencia()
        {
            return View();
        }

        public IActionResult RegistrarIngreso()
        {
            return View();
        }
    }
}