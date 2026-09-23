using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Metum
{
    public int MetaId { get; set; }

    public int ClienteId { get; set; }

    public string Tipo { get; set; } = null!;

    public decimal? ValorObjetivo { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaLimite { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;
}
