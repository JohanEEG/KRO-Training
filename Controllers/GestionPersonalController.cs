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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var colaboradores = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Where(u =>
                    u.Rol.Nombre == "ADMINISTRADOR" ||
                    u.Rol.Nombre == "ENTRENADOR")
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(colaboradores);
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

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    (u.Rol.Nombre == "ADMINISTRADOR" ||
                     u.Rol.Nombre == "ENTRENADOR"));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            var model = new EditarColaboradorViewModel
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                RolId = usuario.RolId
            };

            await CargarRolesAsync(model.RolId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            EditarColaboradorViewModel model)
        {
            // Comprobar que el formulario corresponde al ID de la dirección.
            if (id != model.UsuarioId)
            {
                return BadRequest(
                    "El identificador del colaborador no coincide.");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Entrenador)
                .Include(u => u.Administrador)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    (u.Rol.Nombre == "ADMINISTRADOR" ||
                     u.Rol.Nombre == "ENTRENADOR"));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            // Aceptar únicamente roles de colaboradores.
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

            // El correo puede seguir siendo el del mismo colaborador.
            // Lo que se rechaza es que pertenezca a otra cuenta.
            var existeCorreo = await _context.Usuarios
                .AnyAsync(u =>
                    u.UsuarioId != id &&
                    u.Correo.Trim().ToLower() == correo);

            if (existeCorreo)
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "Este correo pertenece a otra cuenta.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            // Actualizar únicamente los campos permitidos.
            usuario.Nombre = model.Nombre.Trim();
            usuario.Correo = correo;
            usuario.Telefono = string.IsNullOrWhiteSpace(model.Telefono)
                ? null
                : model.Telefono.Trim();
            usuario.RolId = rol.RolId;

            // Crear el registro del nuevo tipo si todavía no existe.
            // Conservar los anteriores para no perder relaciones e historial.
            if (rol.Nombre == "ENTRENADOR" &&
                usuario.Entrenador == null)
            {
                usuario.Entrenador = new Entrenador();
            }

            if (rol.Nombre == "ADMINISTRADOR" &&
                usuario.Administrador == null)
            {
                usuario.Administrador = new Administrador();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var sigueExistiendo = await _context.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.UsuarioId == id);

                if (!sigueExistiendo)
                {
                    return NotFound("El colaborador ya no existe.");
                }

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron guardar los cambios. "
                    + "Recargue la página e inténtelo nuevamente.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sql &&
                      (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar porque existe un dato único duplicado. "
                    + "Compruebe el correo y recargue la página.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            TempData["MensajeExito"] =
                "La información del colaborador se actualizó correctamente.";

            return RedirectToAction(nameof(Editar), new { id });
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