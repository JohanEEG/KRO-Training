using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Entrenador
{
    public int UsuarioId { get; set; }

    public string? Especialidad { get; set; }

    public virtual ICollection<AsignacionRutina> AsignacionRutinas { get; set; } = new List<AsignacionRutina>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<EvaluacionFisica> EvaluacionFisicas { get; set; } = new List<EvaluacionFisica>();

    public virtual ICollection<ObservacionEntrenador> ObservacionEntrenadors { get; set; } = new List<ObservacionEntrenador>();

    public virtual ICollection<Recomendacion> Recomendacions { get; set; } = new List<Recomendacion>();

    public virtual ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();

    public virtual Usuario Usuario { get; set; } = null!;
}
