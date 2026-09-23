using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class SesionEjercicio
{
    public int SesionId { get; set; }

    public int EjercicioId { get; set; }

    public int? SeriesCompletadas { get; set; }

    public int? RepeticionesRealizadas { get; set; }

    public decimal? PesoUtilizado { get; set; }

    public bool Completado { get; set; }

    public DateTime? FechaMarcado { get; set; }

    public string? Observaciones { get; set; }

    public virtual Ejercicio Ejercicio { get; set; } = null!;

    public virtual SesionEntrenamiento Sesion { get; set; } = null!;
}
