namespace KROTraining.Models;

public partial class FichaMedica
{
    public int UsuarioId { get; set; }

    public string? ContactoEmergencia { get; set; }

    public string? Alergias { get; set; }

    public string? Padecimientos { get; set; }

    public string? Medicamentos { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
