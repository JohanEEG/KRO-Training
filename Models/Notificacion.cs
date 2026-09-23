using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Notificacion
{
    public int NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
