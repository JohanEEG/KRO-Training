using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Asistencium
{
    public int AsistenciaId { get; set; }

    public int ClienteId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
