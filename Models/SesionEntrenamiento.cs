using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class SesionEntrenamiento
{
    public int SesionId { get; set; }

    public int AsignacionId { get; set; }

    public DateTime Fecha { get; set; }

    public string Estado { get; set; } = null!;

    public virtual AsignacionRutina Asignacion { get; set; } = null!;

    public virtual ICollection<SesionEjercicio> SesionEjercicios { get; set; } = new List<SesionEjercicio>();
}
