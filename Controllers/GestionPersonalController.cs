using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KRO_Training_Performance.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class GestionPersonalController : Controller
    {
        private readonly KroTrainingContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public GestionPersonalController(
            KroTrainingContext context,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarRolesAsync();
            return View(new RegistrarColaboradorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            RegistrarColaboradorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            var rol = await _context.Rols
                .SingleOrDefaultAsync(r =>
                    r.RolId == model.RolId &&
                    (r.Nombre == "ADMINISTRADOR" ||
                     r.Nombre == "ENTRENADOR"));

            if (rol == null)
            {
                ModelState.AddModelError(
                    nameof(model.RolId),
                    "Seleccione un rol de colaborador válido.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            var correo = model.Correo.Trim().ToLowerInvariant();

            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Correo.Trim().ToLower() == correo);

            if (existeCorreo)
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "Este correo electrónico ya está registrado.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre.Trim(),
                Correo = correo,
                Telefono = string.IsNullOrWhiteSpace(model.Telefono)
                    ? null
                    : model.Telefono.Trim(),
                RolId = rol.RolId,
                Estado = "ACTIVO",
                FechaRegistro = DateTime.Now
            };

            usuario.ContrasenaHash = _passwordHasher.HashPassword(
                usuario,
                model.Contrasena);

            if (rol.Nombre == "ENTRENADOR")
            {
                usuario.Entrenador = new Entrenador();
            }
            else
            {
                usuario.Administrador = new Administrador();
            }

            _context.Usuarios.Add(usuario);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sql &&
                      (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "No se pudo registrar: existe un dato único duplicado. "
                    + "Compruebe el correo electrónico.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            TempData["MensajeExito"] =
                "El colaborador fue registrado correctamente.";

            return RedirectToAction(nameof(Crear));
        }

        public IActionResult Editar()
        {
            return View();
        }

        private async Task CargarRolesAsync(int? seleccionado = null)
        {
            var roles = await _context.Rols
                .AsNoTracking()
                .Where(r =>
                    r.Nombre == "ADMINISTRADOR" ||
                    r.Nombre == "ENTRENADOR")
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            ViewBag.Roles = new SelectList(
                roles,
                "RolId",
                "Nombre",
                seleccionado);
        }
    }
}