using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KROTraining.Models
{
    public class RegistrarEvaluacionInicialViewModel
    {
        [Required(ErrorMessage = "Seleccione un cliente.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un cliente válido.")]
        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }

        [Required(ErrorMessage = "Ingrese el peso del cliente.")]
        [Range(typeof(decimal), "0.01", "9999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El peso debe estar entre 0,01 y 9999,99 kg.")]
        [Display(Name = "Peso (kg)")]
        public decimal? Peso { get; set; }

        [Required(ErrorMessage = "Ingrese la estatura del cliente.")]
        [Range(typeof(decimal), "0.01", "999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "La estatura debe estar entre 0,01 y 999,99 m.")]
        [Display(Name = "Estatura (m)")]
        public decimal? Estatura { get; set; }

        [StringLength(1000,
            ErrorMessage = "Las medidas no pueden superar los 1000 caracteres.")]
        [Display(Name = "Medidas adicionales")]
        public string? Medidas { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> ClientesDisponibles { get; set; }
            = new List<SelectListItem>();
    }
}
