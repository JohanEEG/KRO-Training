using Microsoft.AspNetCore.Mvc;

namespace KROTraining.Controllers
{
    public class ClientesController : Controller
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