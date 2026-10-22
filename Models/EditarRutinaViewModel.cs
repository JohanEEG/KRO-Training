using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class EditarRutinaViewModel : CrearRutinaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador de la rutina no es válido.")]
        public int RutinaId { get; set; }

        public List<EditarEjercicioRutinaViewModel> Ejercicios { get; set; }
            = new List<EditarEjercicioRutinaViewModel>();
    }

    public class EditarEjercicioRutinaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador del ejercicio no es válido.")]
        public int EjercicioId { get; set; }

        [BindNever]
        [ValidateNever]
        public string NombreEjercicio { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public int Orden { get; set; }

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
        public int? DescansoSegundos { get; set; }
    }
}