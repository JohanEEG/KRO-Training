using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public class FiltroHistorialEvaluacionesViewModel
        : IValidatableObject
    {
        [DataType(DataType.Date)]
        [Display(Name = "Desde")]
        public DateTime? FechaInicio { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hasta")]
        public DateTime? FechaFin { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (FechaInicio.HasValue &&
                FechaFin.HasValue &&
                FechaInicio.Value.Date > FechaFin.Value.Date)
            {
                yield return new ValidationResult(
                    "La fecha inicial no puede ser posterior a la fecha final.",
                    new[] { nameof(FechaInicio), nameof(FechaFin) });
            }
        }
    }
}