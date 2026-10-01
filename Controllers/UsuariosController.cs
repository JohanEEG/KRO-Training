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

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            var roles = await _context.Rols
                .AsNoTracking()
                .Include(r => r.Permisos)
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return View(roles);
        }

        public IActionResult NuevoRol()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditarRol(int id)
        {
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            var rol = await _context.Rols
                .AsNoTracking()
                .Include(r => r.Permisos)
                .SingleOrDefaultAsync(r => r.RolId == id);

            if (rol == null)
            {
                return NotFound("El rol no fue encontrado.");
            }

            var permisosDisponibles = await _context.Permisos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new PermisoOpcionViewModel
                {
                    PermisoId = p.PermisoId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();

            var model = new ConfigurarPermisosViewModel
            {
                RolId = rol.RolId,
                NombreRol = rol.Nombre,
                PermisosSeleccionados = rol.Permisos
                    .Select(p => p.PermisoId)
                    .ToList(),
                PermisosDisponibles = permisosDisponibles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRol(
    int id,
    ConfigurarPermisosViewModel model)
        {
            // Comprobar el rol y el estado actual del administrador en la BD.
            if (!await EsAdministradorActivoAsync())
            {
                return Forbid();
            }

            if (id <= 0 || id != model.RolId)
            {
                return BadRequest("El identificador del rol no es válido.");
            }

            var rol = await _context.Rols
                .Include(r => r.Permisos)
                .SingleOrDefaultAsync(r => r.RolId == id);

            if (rol == null)
            {
                return NotFound("El rol no fue encontrado.");
            }

            var catalogo = await _context.Permisos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            // Recuperar los datos informativos desde la base de datos.
            model.NombreRol = rol.Nombre;

            model.PermisosDisponibles = catalogo
                .Select(p => new PermisoOpcionViewModel
                {
                    PermisoId = p.PermisoId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion
                })
                .ToList();

            model.PermisosSeleccionados =
                (model.PermisosSeleccionados ?? new List<int>())
                .Distinct()
                .ToList();

            var seleccionados = model.PermisosSeleccionados.ToHashSet();
            var idsDisponibles = catalogo
                .Select(p => p.PermisoId)
                .ToHashSet();

            if (seleccionados.Any(idPermiso => !idsDisponibles.Contains(idPermiso)))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Uno o más permisos seleccionados no existen. " +
                    "Recargue la página y revise la selección.");
            }

            var nombresSeleccionados = catalogo
                .Where(p => seleccionados.Contains(p.PermisoId))
                .Select(p => p.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Reglas de conflicto acordadas para la HU-66.
            var dependencias = new[]
            {
        (
            Accion: "COLABORADORES_EDITAR",
            Consulta: "COLABORADORES_VER"
        ),
        (
            Accion: "USUARIOS_ASIGNAR_ROL",
            Consulta: "USUARIOS_VER"
        ),
        (
            Accion: "ROLES_CONFIGURAR",
            Consulta: "ROLES_VER"
        )
    };

            foreach (var dependencia in dependencias)
            {
                if (nombresSeleccionados.Contains(dependencia.Accion) &&
                    !nombresSeleccionados.Contains(dependencia.Consulta))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Conflicto de permisos: para asignar " +
                        $"{dependencia.Accion}, también debe seleccionar " +
                        $"{dependencia.Consulta}.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retirar únicamente las asignaciones que se desmarcaron.
            var permisosARetirar = rol.Permisos
                .Where(p => !seleccionados.Contains(p.PermisoId))
                .ToList();

            foreach (var permiso in permisosARetirar)
            {
                rol.Permisos.Remove(permiso);
            }

            // Agregar únicamente las asignaciones que faltan.
            var idsAsignados = rol.Permisos
                .Select(p => p.PermisoId)
                .ToHashSet();

            foreach (var permiso in catalogo)
            {
                if (seleccionados.Contains(permiso.PermisoId) &&
                    !idsAsignados.Contains(permiso.PermisoId))
                {
                    rol.Permisos.Add(permiso);
                }
            }

            try
            {
                // EF guarda estos cambios juntos en una transacción.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar la configuración. " +
                    "Recargue la página e inténtelo nuevamente.");

                return View(model);
            }

            TempData["MensajeExito"] =
                $"Los permisos del rol {rol.Nombre} se guardaron correctamente.";

            return RedirectToAction(nameof(Roles));
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