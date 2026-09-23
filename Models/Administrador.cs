using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Administrador
{
    public int UsuarioId { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual ICollection<ParametroRecomendacion> ParametroRecomendacions { get; set; } = new List<ParametroRecomendacion>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<ValidacionPrediccion> ValidacionPrediccions { get; set; } = new List<ValidacionPrediccion>();
}
