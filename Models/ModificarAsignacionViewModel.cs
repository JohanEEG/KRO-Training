using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KROTraining.Models
{
    public class ModificarAsignacionViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador de la asignación no es válido.")]
        public int AsignacionId { get; set; }

        [BindNever]
        [ValidateNever]
        public string NombreCliente { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string NombreRutinaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione la nueva rutina.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione una rutina válida.")]
        [Display(Name = "Nueva rutina")]
        public int? NuevaRutinaId { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> RutinasDisponibles { get; set; }
            = new List<SelectListItem>();
    }

    public class ClienteAsignacionItemViewModel
    {
        public int ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public int? AsignacionId { get; set; }

        public string? NombreRutina { get; set; }

        public DateTime? FechaAsignacion { get; set; }

        public bool TieneAsignacion => AsignacionId.HasValue;
    }
}
