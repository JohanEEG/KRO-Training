using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public class RegistrarMedidasViewModel
    {
        [Required(ErrorMessage = "Ingrese su peso.")]
        [Range(typeof(decimal), "1", "700",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El peso debe estar entre 1 y 700 kg.")]
        [Display(Name = "Peso (kg)")]
        public decimal? Peso { get; set; }

        [Required(ErrorMessage = "Ingrese su estatura.")]
        [Range(typeof(decimal), "0.30", "3.00",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "La estatura debe estar entre 0,30 y 3,00 metros.")]
        [Display(Name = "Estatura (m)")]
        public decimal? Estatura { get; set; }

        [Range(typeof(decimal), "1", "300",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "La cintura debe estar entre 1 y 300 cm.")]
        [Display(Name = "Cintura (cm)")]
        public decimal? Cintura { get; set; }

        [Range(typeof(decimal), "1", "300",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "La cadera debe estar entre 1 y 300 cm.")]
        [Display(Name = "Cadera (cm)")]
        public decimal? Cadera { get; set; }

        [Range(typeof(decimal), "1", "150",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El brazo debe estar entre 1 y 150 cm.")]
        [Display(Name = "Brazo (cm)")]
        public decimal? Brazo { get; set; }
    }
}