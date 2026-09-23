using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class PrediccionDesercion
{
    public int PrediccionId { get; set; }

    public int ClienteId { get; set; }

    public string NivelRiesgo { get; set; } = null!;

    public decimal Confiabilidad { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ValidacionPrediccion? ValidacionPrediccion { get; set; }
}
