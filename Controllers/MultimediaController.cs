using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KROTraining.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KROTraining.Controllers
{
    [Authorize]
    public class MultimediaController : Controller
    {
        private readonly KroTrainingContext _context;
        private readonly IWebHostEnvironment _environment;

        public MultimediaController(KroTrainingContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ==========================================
        // GALERÍA / ÍNDICE GENERAL DE MULTIMEDIA
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? grupoMuscular)
        {
            var query = _context.Ejercicios
                .Include(e => e.MultimediaEjercicios)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(e => e.Nombre.Contains(buscar) || (e.Descripcion != null && e.Descripcion.Contains(buscar)));
            }

            if (!string.IsNullOrEmpty(grupoMuscular))
            {
                query = query.Where(e => e.GrupoMuscular == grupoMuscular);
            }

            var listaEjercicios = await query
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.GrupoMuscular = grupoMuscular;

            return View(listaEjercicios);
        }

        // ==========================================
        // HU-15: CREAR EJERCICIO
        // ==========================================
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        [HttpGet]
        public IActionResult CrearEjercicio()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEjercicio(Ejercicio ejercicio, List<IFormFile>? archivosMultimedia)
        {
            if (ModelState.IsValid)
            {
                // 1. Guardar el Ejercicio
                _context.Ejercicios.Add(ejercicio);
                await _context.SaveChangesAsync(); // Se guarda para obtener el EjercicioId generado

                // 2. Si el usuario seleccionó archivos en este mismo formulario, procesarlos inmediatamente
                if (archivosMultimedia != null && archivosMultimedia.Count > 0)
                {
                    await ProcesarArchivosMultimedia(ejercicio.EjercicioId, archivosMultimedia);
                    TempData["MensajeExito"] = "Ejercicio y archivos multimedia guardados correctamente.";
                    return RedirectToAction(nameof(VerMultimedia), new { id = ejercicio.EjercicioId });
                }

                TempData["MensajeExito"] = "Ejercicio registrado con éxito. Puedes adjuntar material multimedia.";
                // Redirige a adjuntar archivos para el ejercicio recién creado
                return RedirectToAction(nameof(AdjuntarMultimedia), new { id = ejercicio.EjercicioId });
            }

            return View(ejercicio);
        }

        // ==========================================
        // HU-16: ADJUNTAR MULTIMEDIA (Imágenes / Videos)
        // ==========================================
        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        [HttpGet]
        public async Task<IActionResult> AdjuntarMultimedia(int id)
        {
            var ejercicio = await _context.Ejercicios
                .Include(e => e.MultimediaEjercicios)
                .FirstOrDefaultAsync(e => e.EjercicioId == id);

            if (ejercicio == null)
            {
                return NotFound();
            }

            ViewBag.Ejercicio = ejercicio;
            return View();
        }

        [Authorize(Roles = "ADMINISTRADOR,ENTRENADOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjuntarMultimedia(int ejercicioId, List<IFormFile> archivosMultimedia)
        {
            if (archivosMultimedia != null && archivosMultimedia.Count > 0)
            {
                await ProcesarArchivosMultimedia(ejercicioId, archivosMultimedia);
                TempData["MensajeExito"] = "Multimedia adjuntada correctamente.";
                return RedirectToAction(nameof(VerMultimedia), new { id = ejercicioId });
            }

            ViewBag.Ejercicio = await _context.Ejercicios.FindAsync(ejercicioId);
            ModelState.AddModelError("", "Por favor seleccione al menos un archivo.");
            return View();
        }

        // ==========================================
        // VER MULTIMEDIA DEL EJERCICIO
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> VerMultimedia(int id)
        {
            var ejercicio = await _context.Ejercicios
                .Include(e => e.MultimediaEjercicios)
                .FirstOrDefaultAsync(e => e.EjercicioId == id);

            if (ejercicio == null)
            {
                return NotFound();
            }

            return View(ejercicio);
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================
        private async Task ProcesarArchivosMultimedia(int ejercicioId, List<IFormFile> archivos)
        {
            // Crear la carpeta wwwroot/uploads/ejercicios si no existe
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ejercicios");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    // Generar un nombre único para evitar sobreescribir archivos
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(archivo.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    // Guardar la referencia con las propiedades exactas de tu modelo
                    var multimedia = new MultimediaEjercicio
                    {
                        EjercicioId = ejercicioId,
                        Url = "/uploads/ejercicios/" + uniqueFileName,
                        Tipo = archivo.ContentType.StartsWith("video", StringComparison.OrdinalIgnoreCase) ? "Video" : "Imagen"
                    };

                    _context.MultimediaEjercicios.Add(multimedia);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}