using Microsoft.AspNetCore.Mvc;

namespace KROTraining.Controllers
{
    public class CuentaController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult RecuperarPassword()
        {
            return View();
        }

        public IActionResult RestablecerPassword()
        {
            return View();
        }
    }
}