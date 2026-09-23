using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Membresium
{
    public int MembresiaId { get; set; }

    public int ClienteId { get; set; }

    public int PlanId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual PlanMembresium Plan { get; set; } = null!;
}
