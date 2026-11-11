using System;

namespace KROTraining.Models;

public partial class HorarioPersonal
{
    public int UsuarioId { get; set; }

    // 1 = Lunes ... 7 = Domingo
    public byte DiaSemana { get; set; }

    public string? Turno { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
