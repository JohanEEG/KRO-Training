using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KRO_Training_Performance.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class GestionPersonalController : Controller
    {
        // Roles que se gestionan desde esta pantalla (colaboradores del gimnasio).
        private static readonly string[] RolesColaborador =
        {
            "ADMINISTRADOR",
            "ENTRENADOR",
            "EMPLEADO"
        };

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
                .Where(u => RolesColaborador.Contains(u.Rol.Nombre))
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
                    RolesColaborador.Contains(r.Nombre));

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
            else if (rol.Nombre == "EMPLEADO")
            {
                usuario.Empleado = new Empleado();
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
                    RolesColaborador.Contains(u.Rol.Nombre));

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
            if (id != model.UsuarioId)
            {
                return BadRequest(
                    "El identificador del colaborador no coincide.");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Entrenador)
                .Include(u => u.Administrador)
                .Include(u => u.Empleado)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    RolesColaborador.Contains(u.Rol.Nombre));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            var rol = await _context.Rols
                .SingleOrDefaultAsync(r =>
                    r.RolId == model.RolId &&
                    RolesColaborador.Contains(r.Nombre));

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

            usuario.Nombre = model.Nombre.Trim();
            usuario.Correo = correo;
            usuario.Telefono = string.IsNullOrWhiteSpace(model.Telefono)
                ? null
                : model.Telefono.Trim();
            usuario.RolId = rol.RolId;

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

            if (rol.Nombre == "EMPLEADO" &&
                usuario.Empleado == null)
            {
                usuario.Empleado = new Empleado();
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

        [HttpGet]
        public async Task<IActionResult> DarBaja(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    RolesColaborador.Contains(u.Rol.Nombre));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            var model = new DarBajaColaboradorViewModel
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol.Nombre,
                Estado = usuario.Estado
            };

            if (usuario.Estado == "INACTIVO")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Este colaborador ya está inactivo.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DarBaja(
            int id,
            DarBajaColaboradorViewModel model)
        {
            if (id != model.UsuarioId)
            {
                return BadRequest(
                    "El identificador del colaborador no coincide.");
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    RolesColaborador.Contains(u.Rol.Nombre));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            model.Nombre = usuario.Nombre;
            model.Correo = usuario.Correo;
            model.Rol = usuario.Rol.Nombre;
            model.Estado = usuario.Estado;

            if (usuario.Estado == "INACTIVO")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Este colaborador ya está inactivo.");

                return View(model);
            }

            var identificadorActual =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificadorActual, out var usuarioActualId))
            {
                return Forbid();
            }

            if (usuarioActualId == id)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No puede dar de baja su propia cuenta.");

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var filasActualizadas = await _context.Usuarios
                .Where(u =>
                    u.UsuarioId == id &&
                    u.Estado != "INACTIVO" &&
                    RolesColaborador.Contains(u.Rol.Nombre))
                .ExecuteUpdateAsync(cambios => cambios
                    .SetProperty(u => u.Estado, "INACTIVO"));

            if (filasActualizadas == 0)
            {
                return RedirectToAction(nameof(DarBaja), new { id });
            }

            TempData["MensajeExito"] =
                "El colaborador fue dado de baja correctamente.";

            return RedirectToAction(nameof(Index));
        }

<<<<<<< Updated upstream
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
=======
        [HttpGet]
        public async Task<IActionResult> Activar(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    (u.Rol.Nombre == "ADMINISTRADOR" ||
                     u.Rol.Nombre == "ENTRENADOR"));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            var model = new DarBajaColaboradorViewModel
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol.Nombre,
                Estado = usuario.Estado
            };

            if (usuario.Estado == "ACTIVO")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Este colaborador ya se encuentra activo.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(
            int id,
            DarBajaColaboradorViewModel model)
        {
            if (id != model.UsuarioId)
            {
                return BadRequest(
                    "El identificador del colaborador no coincide.");
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u =>
                    u.UsuarioId == id &&
                    (u.Rol.Nombre == "ADMINISTRADOR" ||
                     u.Rol.Nombre == "ENTRENADOR"));

            if (usuario == null)
            {
                return NotFound("El colaborador no fue encontrado.");
            }

            model.Nombre = usuario.Nombre;
            model.Correo = usuario.Correo;
            model.Rol = usuario.Rol.Nombre;
            model.Estado = usuario.Estado;

            if (usuario.Estado == "ACTIVO")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Este colaborador ya se encuentra activo.");

                return View(model);
            }

>>>>>>> Stashed changes
            var filasActualizadas = await _context.Usuarios
                .Where(u =>
                    u.UsuarioId == id &&
                    u.Estado == "INACTIVO" &&
<<<<<<< Updated upstream
                    RolesColaborador.Contains(u.Rol.Nombre))
=======
                    (u.Rol.Nombre == "ADMINISTRADOR" ||
                     u.Rol.Nombre == "ENTRENADOR"))
>>>>>>> Stashed changes
                .ExecuteUpdateAsync(cambios => cambios
                    .SetProperty(u => u.Estado, "ACTIVO"));

            if (filasActualizadas == 0)
            {
<<<<<<< Updated upstream
                TempData["Error"] =
                    "No se pudo reactivar: el colaborador no existe o ya está activo.";
            }
            else
            {
                TempData["MensajeExito"] =
                    "El colaborador fue reactivado correctamente.";
            }

=======
                return RedirectToAction(nameof(Activar), new { id });
            }

            TempData["MensajeExito"] =
                "El colaborador fue dado de alta (activado) correctamente.";

>>>>>>> Stashed changes
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesAsync(int? seleccionado = null)
        {
            var roles = await _context.Rols
                .AsNoTracking()
                .Where(r => RolesColaborador.Contains(r.Nombre))
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
