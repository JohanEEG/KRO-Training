namespace KROTraining.Models
{
    public class HorarioPersonalViewModel
    {
        public static readonly string[] NombresDias =
        {
            "Lunes",
            "Martes",
            "Miércoles",
            "Jueves",
            "Viernes",
            "Sábado",
            "Domingo"
        };

        public const int MaxLongitudTurno = 100;

        public bool PuedeEditar { get; set; }

        public List<FilaHorarioViewModel> Filas { get; set; } = new();
    }

    public class TurnoHoyViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string Turno { get; set; } = string.Empty;
    }

    public class FilaHorarioViewModel
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        // Índice 0 = Lunes ... 6 = Domingo
        public string?[] Dias { get; set; } = new string?[7];
    }
}
