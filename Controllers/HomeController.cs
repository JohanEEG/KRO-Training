using KROTraining.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace KROTraining.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] RolesPersonal =
        {
            "ADMINISTRADOR",
            "ENTRENADOR",
            "EMPLEADO"
        };

        private readonly KroTrainingContext _context;

        public HomeController(KroTrainingContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // 1 = Lunes ... 7 = Domingo (igual que HORARIO_PERSONAL.dia_semana).
            int diaHoy = ((int)DateTime.Today.DayOfWeek + 6) % 7 + 1;
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var vm = new DashboardViewModel
            {
                DiaHoy = HorarioPersonalViewModel.NombresDias[diaHoy - 1]
            };

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int usuarioId);

            try
            {
                // El equipo en turno solo lo ve el personal.
                if (User.IsInRole("ADMINISTRADOR") || User.IsInRole("ENTRENADOR") || User.IsInRole("EMPLEADO"))
                {
                    var turnos = await _context.HorarioPersonals
                        .AsNoTracking()
                        .Include(h => h.Usuario)
                        .ThenInclude(u => u.Rol)
                        .Where(h => h.DiaSemana == diaHoy &&
                                    h.Usuario.Estado == "ACTIVO" &&
                                    RolesPersonal.Contains(h.Usuario.Rol.Nombre) &&
                                    h.Turno != null &&
                                    h.Turno.Trim() != "" &&
                                    !h.Turno.ToLower().Contains("descanso"))
                        .Select(h => new TurnoHoyViewModel
                        {
                            Nombre = h.Usuario.Nombre,
                            Rol = h.Usuario.Rol.Nombre,
                            Turno = h.Turno!
                        })
                        .OrderBy(t => t.Nombre)
                        .ToListAsync();

                    vm.Administradores = turnos.Where(t => t.Rol == "ADMINISTRADOR").ToList();
                    vm.Entrenadores = turnos.Where(t => t.Rol == "ENTRENADOR").ToList();
                    vm.Empleados = turnos.Where(t => t.Rol == "EMPLEADO").ToList();
                }

                if (User.IsInRole("ENTRENADOR"))
                {
                    vm.ClientesAsignados = await _context.Clientes
                        .AsNoTracking()
                        .Include(c => c.Usuario)
                        .Where(c => c.EntrenadorId == usuarioId && c.Usuario.Estado == "ACTIVO")
                        .OrderBy(c => c.Usuario.Nombre)
                        .Select(c => new ClienteAsignadoViewModel
                        {
                            Nombre = c.Usuario.Nombre,
                            BadgeNumero = c.BadgeNumero,
                            Nivel = c.Nivel,
                            Objetivo = c.Objetivo
                        })
                        .ToListAsync();
                }

                if (User.IsInRole("CLIENTE"))
                {
                    vm.EntrenadorAsignado = await _context.Clientes
                        .AsNoTracking()
                        .Where(c => c.UsuarioId == usuarioId && c.EntrenadorId != null)
                        .Select(c => new EntrenadorAsignadoViewModel
                        {
                            Nombre = c.Entrenador!.Usuario.Nombre,
                            Especialidad = c.Entrenador!.Especialidad
                        })
                        .FirstOrDefaultAsync();
                }

                if (User.IsInRole("EMPLEADO"))
                {
                    vm.TareasProximas = await _context.HorarioTareas
                        .AsNoTracking()
                        .Where(t => t.UsuarioId == usuarioId && t.Fecha >= hoy)
                        .OrderBy(t => t.Fecha)
                        .ToListAsync();
                }
            }
            catch
            {
                vm.ErrorEquipo = "No se pudo cargar la información de hoy. Intente nuevamente más tarde.";
            }

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
