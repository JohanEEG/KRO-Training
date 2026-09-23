using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class MultimediaEjercicio
{
    public int MultimediaId { get; set; }

    public int EjercicioId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Url { get; set; } = null!;

    public virtual Ejercicio Ejercicio { get; set; } = null!;
}
