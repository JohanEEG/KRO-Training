using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Cliente
{
    public int UsuarioId { get; set; }

    public string? BadgeNumero { get; set; }

    public string? Objetivo { get; set; }

    public string? Nivel { get; set; }

    public int? EntrenadorId { get; set; }

    public virtual ICollection<AsignacionRutina> AsignacionRutinas { get; set; } = new List<AsignacionRutina>();

    public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();

    public virtual ICollection<CalificacionRecomendacion> CalificacionRecomendacions { get; set; } = new List<CalificacionRecomendacion>();

    public virtual Entrenador? Entrenador { get; set; }

    public virtual ICollection<EvaluacionFisica> EvaluacionFisicas { get; set; } = new List<EvaluacionFisica>();

    public virtual ICollection<InscripcionEvento> InscripcionEventos { get; set; } = new List<InscripcionEvento>();

    public virtual ICollection<Membresium> Membresia { get; set; } = new List<Membresium>();

    public virtual ICollection<Metum> Meta { get; set; } = new List<Metum>();

    public virtual ICollection<ObservacionEntrenador> ObservacionEntrenadors { get; set; } = new List<ObservacionEntrenador>();

    public virtual ICollection<PrediccionDesercion> PrediccionDesercions { get; set; } = new List<PrediccionDesercion>();

    public virtual ICollection<Recomendacion> Recomendacions { get; set; } = new List<Recomendacion>();

    public virtual Usuario Usuario { get; set; } = null!;
}
