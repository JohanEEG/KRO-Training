using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public class EditarColaboradorViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador del colaborador no es válido.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre completo.")]
        [StringLength(150,
            ErrorMessage = "El nombre admite hasta 150 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese el correo electrónico.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150,
            ErrorMessage = "El correo admite hasta 150 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(30,
            ErrorMessage = "El teléfono admite hasta 30 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un rol válido.")]
        [Display(Name = "Rol")]
        public int? RolId { get; set; }
    }
}
