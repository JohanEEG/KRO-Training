using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class HorarioTarea
{
    public int HorarioId { get; set; }

    public int UsuarioId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Tarea { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
