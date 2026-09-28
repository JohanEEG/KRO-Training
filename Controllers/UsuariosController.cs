using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KROTraining.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UsuariosController : Controller
    {
        private readonly KroTrainingContext _context;

        public UsuariosController(KroTrainingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> AsignarRol(int id)
        {
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound("El usuario no fue encontrado.");
            }

            var model = new AsignarRolViewModel
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                RolActual = usuario.Rol.Nombre,
                RolId = usuario.RolId
            };

            await CargarRolesAsync(model.RolId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(
            int id,
            AsignarRolViewModel model)
        {
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            if (id != model.UsuarioId)
            {
                return BadRequest(
                    "El identificador del usuario no coincide.");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Administrador)
                .Include(u => u.Entrenador)
                .Include(u => u.Cliente)
                .SingleOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound("El usuario no fue encontrado.");
            }

            // Los datos informativos se recuperan de la base de datos.
            model.Nombre = usuario.Nombre;
            model.Correo = usuario.Correo;
            model.RolActual = usuario.Rol.Nombre;

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            // Validar en el servidor que el rol seleccionado exista.
            var rol = await _context.Rols
                .SingleOrDefaultAsync(r => r.RolId == model.RolId);

            if (rol == null)
            {
                ModelState.AddModelError(
                    nameof(model.RolId),
                    "El rol seleccionado no existe.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            var identificadorActual =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificadorActual, out var usuarioActualId))
            {
                return Forbid();
            }

            // Evitar que el administrador se quite su propio acceso.
            if (usuario.UsuarioId == usuarioActualId &&
                rol.Nombre != "ADMINISTRADOR")
            {
                ModelState.AddModelError(
                    nameof(model.RolId),
                    "No puede quitarse su propio rol de administrador.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            usuario.RolId = rol.RolId;

            // Crear la relación necesaria si todavía no existe.
            // Las relaciones anteriores se conservan para mantener el historial.
            if (rol.Nombre == "ADMINISTRADOR" &&
                usuario.Administrador == null)
            {
                usuario.Administrador = new Administrador();
            }

            if (rol.Nombre == "ENTRENADOR" &&
                usuario.Entrenador == null)
            {
                usuario.Entrenador = new Entrenador();
            }

            if (rol.Nombre == "CLIENTE" &&
                usuario.Cliente == null)
            {
                usuario.Cliente = new Cliente();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El usuario cambió o dejó de existir. " +
                    "Vuelva al listado y revise sus datos.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar la asignación. " +
                    "Recargue la página e inténtelo nuevamente.");

                await CargarRolesAsync(model.RolId);
                return View(model);
            }

            TempData["MensajeExito"] =
                "El rol del usuario se guardó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Editar()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return View();
        }

        public IActionResult NuevoRol()
        {
            return View();
        }

        public IActionResult EditarRol()
        {
            return View();
        }

        private async Task CargarRolesAsync(int? seleccionado = null)
        {
            var roles = await _context.Rols
                .AsNoTracking()
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            ViewBag.Roles = new SelectList(
                roles,
                "RolId",
                "Nombre",
                seleccionado);
        }

        private async Task<bool> EsAdministradorActivoAsync()
        {
            var identificador =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(identificador, out var usuarioId))
            {
                return false;
            }

            return await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Estado == "ACTIVO" &&
                    u.Rol.Nombre == "ADMINISTRADOR");
        }
    }
}