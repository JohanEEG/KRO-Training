using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public int MembresiaId { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; }

    public string MetodoPago { get; set; } = null!;

    public int RegistradoPor { get; set; }

    public virtual Membresium Membresia { get; set; } = null!;

    public virtual Usuario RegistradoPorNavigation { get; set; } = null!;
}
