using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class RegistrarEvaluacionPeriodicaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El cliente seleccionado no es válido.")]
        public int ClienteId { get; set; }

        [BindNever]
        [ValidateNever]
        public string NombreCliente { get; set; } = string.Empty;

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
    }
}