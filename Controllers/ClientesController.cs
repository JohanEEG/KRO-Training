using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KROTraining.Controllers
{
    public class ClientesController : Controller
    {
        private readonly KroTrainingContext _context;

        public ClientesController(KroTrainingContext context)
        {
            _context = context;
        }

        // GET: Clientes (Búsqueda por nombre, cédula o número de badge)
        public async Task<IActionResult> Index(string buscar)
        {
            var query = _context.Clientes
                .Include(c => c.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(c => (c.Usuario != null && c.Usuario.Nombre.Contains(buscar))
                                      || (c.Usuario != null && c.Usuario.Cedula != null && c.Usuario.Cedula.Contains(buscar))
                                      || (c.BadgeNumero != null && c.BadgeNumero.Contains(buscar)));
            }

            var clientes = await query.ToListAsync();
            return View(clientes);
        }

        // GET: Clientes/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Clientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string Nombre, string PrimerApellido, string SegundoApellido, string Cedula, string BadgeNumero, string Telefono, string Correo)
        {
            if (ModelState.IsValid)
            {
                string nombreCompleto = $"{Nombre} {PrimerApellido} {SegundoApellido}".Trim();

                // 1. Crear registro en la tabla USUARIO (incluye Cédula)
                var nuevoUsuario = new Usuario
                {
                    Nombre = nombreCompleto,
                    Cedula = Cedula,
                    Correo = Correo,
                    Telefono = Telefono,
                    ContrasenaHash = "Temp123!",
                    Estado = "Activo",
                    FechaRegistro = DateTime.Now,
                    RolId = 3 // ID del Rol Cliente
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // 2. Crear registro en la tabla CLIENTE (incluye BadgeNumero)
                var nuevoCliente = new Cliente
                {
                    UsuarioId = nuevoUsuario.UsuarioId,
                    BadgeNumero = BadgeNumero
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: Clientes/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int UsuarioId, string Nombre, string Cedula, string BadgeNumero, string Telefono, string Correo)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.UsuarioId == UsuarioId);

            if (cliente != null)
            {
                if (cliente.Usuario != null)
                {
                    cliente.Usuario.Nombre = Nombre;
                    cliente.Usuario.Cedula = Cedula;
                    cliente.Usuario.Telefono = Telefono;
                    cliente.Usuario.Correo = Correo;
                }

                cliente.BadgeNumero = BadgeNumero;

                _context.Update(cliente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // POST: Clientes/Desactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Estado = "Inactivo";
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Clientes/Reactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Estado = "Activo";
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}