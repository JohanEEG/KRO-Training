using System;

namespace KROTraining.Models;

public partial class Visitante
{
    public int VisitanteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool ConvertidoCliente { get; set; }

    public int? UsuarioId { get; set; }

    public virtual Usuario? Usuario { get; set; }
}