using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class AsignarRolViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador del usuario no es válido.")]
        public int UsuarioId { get; set; }

        [BindNever]
        [ValidateNever]
        public string Nombre { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string Correo { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string RolActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un rol válido.")]
        [Display(Name = "Nuevo rol")]
        public int? RolId { get; set; }
    }
}