using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Empleado
{
    public int UsuarioId { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
