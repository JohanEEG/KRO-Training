using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Ejercicio
{
    public int EjercicioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? GrupoMuscular { get; set; }

    public virtual ICollection<MultimediaEjercicio> MultimediaEjercicios { get; set; } = new List<MultimediaEjercicio>();

    public virtual ICollection<RutinaEjercicio> RutinaEjercicios { get; set; } = new List<RutinaEjercicio>();

    public virtual ICollection<SesionEjercicio> SesionEjercicios { get; set; } = new List<SesionEjercicio>();
}
