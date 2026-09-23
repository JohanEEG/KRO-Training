using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class ObservacionEntrenador
{
    public int ObservacionId { get; set; }

    public int ClienteId { get; set; }

    public int EntrenadorId { get; set; }

    public DateTime Fecha { get; set; }

    public string Contenido { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Entrenador Entrenador { get; set; } = null!;
}
