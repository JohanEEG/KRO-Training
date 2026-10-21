using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public class CrearRutinaViewModel
    {
        [Required(ErrorMessage = "Ingrese el nombre de la rutina.")]
        [StringLength(150,
            ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        [Display(Name = "Nombre de la rutina")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(100,
            ErrorMessage = "El objetivo no puede superar los 100 caracteres.")]
        [Display(Name = "Objetivo")]
        public string? Objetivo { get; set; }

        [StringLength(30,
            ErrorMessage = "El nivel no puede superar los 30 caracteres.")]
        [Display(Name = "Nivel")]
        public string? Nivel { get; set; }
    }
}
