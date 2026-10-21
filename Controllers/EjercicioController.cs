using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;

namespace KROTraining.Controllers
{
    public class EjercicioController : Controller
    {
        private readonly KroTrainingContext _context;
        private readonly IWebHostEnvironment _environment;

        public EjercicioController(KroTrainingContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Ejercicio/Index
        public async Task<IActionResult> Index(string busqueda)
        {
            var query = _context.Ejercicios
                .Include(e => e.MultimediaEjercicios)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(e => e.Nombre.Contains(busqueda) || e.GrupoMuscular.Contains(busqueda));
            }

            return View(await query.ToListAsync());
        }

        // GET: Ejercicio/Crear (HU-15)
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Ejercicio/Crear (HU-15 y HU-16)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Ejercicio ejercicio, List<IFormFile> archivosMultimedia)
        {
            if (ModelState.IsValid)
            {
                // 1. Guardar el ejercicio principal
                _context.Ejercicios.Add(ejercicio);
                await _context.SaveChangesAsync(); // Guarda para generar el EjercicioId

                // 2. Procesar y guardar los archivos multimedia asociados (HU-16)
                if (archivosMultimedia != null && archivosMultimedia.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/ejercicios");
                    Directory.CreateDirectory(uploadsFolder); // Asegura que la carpeta exista

                    foreach (var archivo in archivosMultimedia)
                    {
                        if (archivo.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await archivo.CopyToAsync(fileStream);
                            }

                            // Crear el registro en la tabla MultimediaEjercicio con las propiedades exactas (Url y Tipo)
                            var multimedia = new MultimediaEjercicio
                            {
                                EjercicioId = ejercicio.EjercicioId,
                                Url = "/uploads/ejercicios/" + uniqueFileName,
                                Tipo = archivo.ContentType.StartsWith("video") ? "Video" : "Imagen"
                            };

                            _context.MultimediaEjercicios.Add(multimedia);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(ejercicio);
        }
    }
}