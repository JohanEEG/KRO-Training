using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class PlanMembresium
{
    public int PlanId { get; set; }

    public string Nombre { get; set; } = null!;

    public int DuracionMeses { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<Membresium> Membresia { get; set; } = new List<Membresium>();
}
