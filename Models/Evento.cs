using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Evento
{
    public int EventoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public int CupoMaximo { get; set; }

    public string Estado { get; set; } = null!;

    public int AdministradorId { get; set; }

    public virtual Administrador Administrador { get; set; } = null!;

    public virtual ICollection<InscripcionEvento> InscripcionEventos { get; set; } = new List<InscripcionEvento>();
}
