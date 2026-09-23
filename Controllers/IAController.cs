using Microsoft.AspNetCore.Mvc;

namespace KRO_Training_Performance.Controllers
{
    public class IAController : Controller
    {
        public IActionResult DashboardMetas()
        {
            return View();
        }

        public IActionResult Recomendaciones()
        {
            return View();
        }

        public IActionResult ValidacionIA()
        {
            return View();
        }
    }
}