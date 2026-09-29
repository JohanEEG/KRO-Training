using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KROTraining.Controllers
{
    public class VisitantesController : Controller
    {
        // HU-12 — Consultar información general del gimnasio
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Informacion()
        {
            return View();
        }
    }
}