using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Reporte
{
    public int ReporteId { get; set; }

    public string Tipo { get; set; } = null!;

    public DateOnly? PeriodoInicio { get; set; }

    public DateOnly? PeriodoFin { get; set; }

    public int GeneradoPor { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public string? FormatoExportado { get; set; }

    public virtual Usuario GeneradoPorNavigation { get; set; } = null!;
}
