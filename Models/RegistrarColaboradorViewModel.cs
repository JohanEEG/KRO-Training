using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public class RegistrarColaboradorViewModel
    {
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

        [StringLength(30,
            ErrorMessage = "El teléfono admite hasta 30 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un rol válido.")]
        [Display(Name = "Rol")]
        public int? RolId { get; set; }

        [Required(ErrorMessage = "Ingrese una contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}