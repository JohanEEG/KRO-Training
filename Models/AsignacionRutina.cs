using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class AsignacionRutina
{
    public int AsignacionId { get; set; }

    public int ClienteId { get; set; }

    public int RutinaId { get; set; }

    public int EntrenadorId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Entrenador Entrenador { get; set; } = null!;

    public virtual Rutina Rutina { get; set; } = null!;

    public virtual ICollection<SesionEntrenamiento> SesionEntrenamientos { get; set; } = new List<SesionEntrenamiento>();
}
