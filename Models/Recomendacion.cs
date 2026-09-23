using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Recomendacion
{
    public int RecomendacionId { get; set; }

    public int ClienteId { get; set; }

    public int RutinaSugeridaId { get; set; }

    public int? EntrenadorId { get; set; }

    public DateTime Fecha { get; set; }

    public string Estado { get; set; } = null!;

    public virtual CalificacionRecomendacion? CalificacionRecomendacion { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Entrenador? Entrenador { get; set; }

    public virtual Rutina RutinaSugerida { get; set; } = null!;
}
