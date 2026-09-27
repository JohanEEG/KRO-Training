using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class DarBajaColaboradorViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador del colaborador no es válido.")]
        public int UsuarioId { get; set; }

        [BindNever]
        [ValidateNever]
        public string Nombre { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string Correo { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string Rol { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string Estado { get; set; } = string.Empty;

        [Display(Name = "Confirmo la baja de este colaborador")]
        [Range(typeof(bool), "true", "true",
            ErrorMessage = "Debe confirmar la baja para continuar.")]
        public bool ConfirmarBaja { get; set; }
    }
}