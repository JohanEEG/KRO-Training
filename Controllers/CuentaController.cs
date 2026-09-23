using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using KROTraining.Models;
using System.Net;
using System.Net.Mail;

namespace KROTraining.Controllers
{
    public class CuentaController : Controller
    {
        private readonly KroTrainingContext _context;

        // Inyectamos el contexto de la base de datos
        public CuentaController(KroTrainingContext context)
        {
            _context = context;
        }

        // GET: /Cuenta/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Cuenta/Login (HU-10)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError(string.Empty, "Por favor, complete todos los campos.");
                return View();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null || usuario.ContrasenaHash != contrasena)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Cliente")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // POST: /Cuenta/Logout (HU-11)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Cuenta");
        }

        // ==========================================
        // REGISTRO DE NUEVA CUENTA
        // ==========================================

        // GET: /Cuenta/Registro
        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Cuenta/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(string nombre, string correo, string contrasena, string? telefono)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError(string.Empty, "Por favor, complete los campos obligatorios.");
                return View();
            }

            // Validar si el correo ya está registrado
            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == correo);
            if (existeCorreo)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico ya se encuentra registrado.");
                return View();
            }

            // Crear el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                ContrasenaHash = contrasena,
                Telefono = telefono,
                RolId = 2 // Asegúrese de que este ID exista en su tabla Rol (ej. "Cliente")
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cuenta creada exitosamente. Ahora puede iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // ==========================================
        // PERFIL Y DATOS PERSONALES (HU-4, HU-5, HU-6)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdString) || !int.TryParse(usuarioIdString, out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
                .Include(u => u.Entrenador)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(string nombre, string? telefono, string? fotoPerfil)
        {
            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdString) || !int.TryParse(usuarioIdString, out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Nombre = nombre;
            usuario.Telefono = telefono;

            if (!string.IsNullOrEmpty(fotoPerfil))
            {
                usuario.FotoPerfil = fotoPerfil;
            }

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // ==========================================
        // SEGURIDAD Y CONTRASEÑAS (HU-7, HU-8, HU-9)
        // ==========================================

        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        // POST: /Cuenta/RecuperarPassword (HU-7)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                TempData["Error"] = "Por favor, ingrese su correo electrónico.";
                return View();
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario != null)
            {
                try
                {
                    // Configuración del servidor SMTP de Gmail con la cuenta oficial
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("krotraining.notificaciones@gmail.com", "pcjuxxpbntmdxmjn"),
                        EnableSsl = true,
                        UseDefaultCredentials = false
                    };

                    // URL dinámica apuntando a la vista de restablecimiento con el correo como parámetro
                    string urlRestablecer = $"https://{HttpContext.Request.Host}/Cuenta/RestablecerPassword?correo={WebUtility.UrlEncode(correo)}";

                    // Diseño HTML profesional para el correo corporativo con enlace al formulario de nueva contraseña
                    string cuerpoHtml = $@"
                    <!DOCTYPE html>
                    <html lang='es'>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 0; }}
                            .email-wrapper {{ width: 100%; background-color: #f4f6f9; padding: 40px 0; }}
                            .email-content {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }}
                            .email-header {{ background-color: #111111; color: #ffffff; text-align: center; padding: 30px; }}
                            .email-header h1 {{ margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 1px; }}
                            .email-header h1 span {{ color: #e63946; }}
                            .email-header p {{ margin: 5px 0 0; font-size: 12px; color: #aaaaaa; letter-spacing: 2px; }}
                            .email-body {{ padding: 40px 30px; color: #333333; line-height: 1.6; }}
                            .email-body h2 {{ color: #111111; font-size: 20px; margin-top: 0; }}
                            .btn-container {{ text-align: center; margin: 30px 0; }}
                            .btn {{ background-color: #e63946; color: #ffffff !important; padding: 12px 30px; border-radius: 4px; text-decoration: none; font-weight: 600; display: inline-block; font-size: 14px; }}
                            .email-footer {{ background-color: #f8f9fa; text-align: center; padding: 20px; font-size: 12px; color: #888888; border-top: 1px solid #eeeeee; }}
                        </style>
                    </head>
                    <body>
                        <div class='email-wrapper'>
                            <div class='email-content'>
                                <div class='email-header'>
                                    <h1>KR<span>Ò</span></h1>
                                    <p>TRAINING</p>
                                </div>
                                <div class='email-body'>
                                    <h2>Restablecimiento de contraseña</h2>
                                    <p>Estimado/a <strong>{usuario.Nombre}</strong>,</p>
                                    <p>Hemos recibido una solicitud para cambiar la contraseña asociada a su cuenta en <strong>KRÒ Training</strong>.</p>
                                    <p>Para establecer una nueva contraseña de acceso de forma segura, haga clic en el siguiente botón:</p>
                                    <div class='btn-container'>
                                        <a href='{urlRestablecer}' class='btn'>Cambiar mi contraseña</a>
                                    </div>
                                    <p style='font-size: 13px; color: #666;'>Si usted no solicitó este cambio, puede ignorar este mensaje de forma segura; su cuenta permanece protegida.</p>
                                </div>
                                <div class='email-footer'>
                                    <p>&copy; 2026 KRÒ Training. Todos los derechos reservados.</p>
                                    <p>Este es un correo automático, por favor no responda a este mensaje.</p>
                                </div>
                            </div>
                        </div>
                    </body>
                    </html>";

                    var mensaje = new MailMessage
                    {
                        From = new MailAddress("krotraining.notificaciones@gmail.com", "KRÒ Training - Soporte"),
                        Subject = "Restablecimiento de contraseña - KRÒ Training",
                        Body = cuerpoHtml,
                        IsBodyHtml = true,
                    };

                    mensaje.To.Add(correo);
                    await smtpClient.SendMailAsync(mensaje);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al enviar el correo: {ex.Message}";
                    return View();
                }
            }

            TempData["Mensaje"] = "Si el correo está registrado, se han enviado las instrucciones a su bandeja de entrada.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Cuenta/RestablecerPassword (Recibe el correo desde el enlace y muestra el formulario)
        [HttpGet]
        public IActionResult RestablecerPassword(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                TempData["Error"] = "El enlace de recuperación no es válido.";
                return RedirectToAction("Login");
            }

            var model = new RestablecerPasswordViewModel { Correo = correo };
            return View(model);
        }

        // POST: /Cuenta/RestablecerPassword (Procesa la nueva contraseña utilizando el ViewModel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Correo) || string.IsNullOrEmpty(model.NuevaContrasena) || string.IsNullOrEmpty(model.ConfirmarContrasena))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                return View(model);
            }

            if (model.NuevaContrasena != model.ConfirmarContrasena)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == model.Correo);
            if (usuario == null)
            {
                TempData["Error"] = "No se encontró una cuenta asociada a este correo.";
                return View(model);
            }

            // Actualizamos la contraseña en la base de datos
            usuario.ContrasenaHash = model.NuevaContrasena;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "¡Contraseña actualizada con éxito! Ya puede iniciar sesión con su nueva clave.";
            return RedirectToAction(nameof(Login));
        }

        // POST: /Cuenta/CambiarPassword (HU-8)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string passwordActual, string nuevoPassword)
        {
            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdString) || !int.TryParse(usuarioIdString, out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || usuario.ContrasenaHash != passwordActual)
            {
                TempData["Error"] = "La contraseña actual es incorrecta.";
                return RedirectToAction(nameof(Perfil));
            }

            usuario.ContrasenaHash = nuevoPassword;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Contraseña actualizada exitosamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // POST: /Cuenta/ActualizarCredenciales (HU-9)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCredenciales(string nuevoCorreo)
        {
            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdString) || !int.TryParse(usuarioIdString, out int usuarioId))
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoCorreo && u.UsuarioId != usuarioId);
            if (existeCorreo)
            {
                TempData["Error"] = "El correo electrónico ya está en uso por otro usuario.";
                return RedirectToAction(nameof(Perfil));
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Correo = nuevoCorreo;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Credenciales de acceso actualizadas correctamente. Por favor, vuelva a iniciar sesión.";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Cuenta");
        }
    }
}