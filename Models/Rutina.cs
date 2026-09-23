using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Rutina
{
    public int RutinaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Objetivo { get; set; }

    public string? Nivel { get; set; }

    public int EntrenadorId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<AsignacionRutina> AsignacionRutinas { get; set; } = new List<AsignacionRutina>();

    public virtual Entrenador Entrenador { get; set; } = null!;

    public virtual ICollection<Recomendacion> Recomendacions { get; set; } = new List<Recomendacion>();

    public virtual ICollection<RutinaEjercicio> RutinaEjercicios { get; set; } = new List<RutinaEjercicio>();
}
