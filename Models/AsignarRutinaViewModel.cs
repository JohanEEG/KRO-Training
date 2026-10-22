using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KROTraining.Models
{
    public class AsignarRutinaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador de la rutina no es válido.")]
        public int RutinaId { get; set; }

        [BindNever]
        [ValidateNever]
        public string NombreRutina { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string? ObjetivoRutina { get; set; }

        [BindNever]
        [ValidateNever]
        public string? NivelRutina { get; set; }

        [Required(ErrorMessage = "Seleccione un cliente.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un cliente válido.")]
        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> ClientesDisponibles { get; set; }
            = new List<SelectListItem>();
    }
}
