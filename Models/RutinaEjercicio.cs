using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class RutinaEjercicio
{
    public int RutinaId { get; set; }

    public int EjercicioId { get; set; }

    public int Orden { get; set; }

    public int Series { get; set; }

    public int Repeticiones { get; set; }

    public int DescansoSegundos { get; set; }

    public virtual Ejercicio Ejercicio { get; set; } = null!;

    public virtual Rutina Rutina { get; set; } = null!;
}
