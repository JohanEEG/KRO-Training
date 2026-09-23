using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class ValidacionPrediccion
{
    public int ValidacionId { get; set; }

    public int PrediccionId { get; set; }

    public int AdministradorId { get; set; }

    public string Resultado { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Administrador Administrador { get; set; } = null!;

    public virtual PrediccionDesercion Prediccion { get; set; } = null!;
}
