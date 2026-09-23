using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class InscripcionEvento
{
    public int EventoId { get; set; }

    public int ClienteId { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Evento Evento { get; set; } = null!;
}
