using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Asistencium
{
    public int AsistenciaId { get; set; }

    public int ClienteId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    /// <summary>
    /// Tipo de actividad o clase a la que ingresa el cliente (Ej. Spinning, Musculación, Crossfit, etc.)
    /// </summary>
    public string? TipoActividad { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}