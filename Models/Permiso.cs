using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Permiso
{
    public int PermisoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Rol> Rols { get; set; } = new List<Rol>();
}
