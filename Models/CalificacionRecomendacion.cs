using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class CalificacionRecomendacion
{
    public int CalificacionId { get; set; }

    public int RecomendacionId { get; set; }

    public int ClienteId { get; set; }

    public int Valor { get; set; }

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Recomendacion Recomendacion { get; set; } = null!;
}
