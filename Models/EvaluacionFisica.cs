using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class EvaluacionFisica
{
    public int EvaluacionId { get; set; }

    public int ClienteId { get; set; }

    public int EntrenadorId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal? Peso { get; set; }

    public decimal? Estatura { get; set; }

    public string? Medidas { get; set; }

    public string? Observaciones { get; set; }

    public string? Tipo { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Entrenador Entrenador { get; set; } = null!;
}
