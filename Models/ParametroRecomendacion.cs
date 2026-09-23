using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class ParametroRecomendacion
{
    public int ParametroId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public int AdministradorId { get; set; }

    public virtual Administrador Administrador { get; set; } = null!;
}
