using System;
using System.Collections.Generic;

namespace KROTraining.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? FotoPerfil { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public int RolId { get; set; }

    public virtual Administrador? Administrador { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual Cliente? Cliente { get; set; }

    public virtual Entrenador? Entrenador { get; set; }

    public virtual ICollection<HorarioTarea> HorarioTareas { get; set; } = new List<HorarioTarea>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual Rol Rol { get; set; } = null!;
}
