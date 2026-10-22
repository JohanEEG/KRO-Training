using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KROTraining.Models
{
    public class AgregarEjercicioRutinaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador de la rutina no es válido.")]
        public int RutinaId { get; set; }

        [BindNever]
        [ValidateNever]
        public string NombreRutina { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un ejercicio.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un ejercicio válido.")]
        [Display(Name = "Ejercicio")]
        public int? EjercicioId { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad de series.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Las series deben ser mayores que cero.")]
        [Display(Name = "Series")]
        public int? Series { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad de repeticiones.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Las repeticiones deben ser mayores que cero.")]
        [Display(Name = "Repeticiones")]
        public int? Repeticiones { get; set; }

        [Required(ErrorMessage = "Ingrese el descanso en segundos.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "El descanso no puede ser negativo.")]
        [Display(Name = "Descanso en segundos")]
        public int? DescansoSegundos { get; set; } = 60;

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> EjerciciosDisponibles { get; set; }
            = new List<SelectListItem>();
    }
}
