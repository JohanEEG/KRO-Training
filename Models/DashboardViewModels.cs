namespace KROTraining.Models
{
    public static class EstadosTarea
    {
        public const string Pendiente = "PENDIENTE";
        public const string Realizada = "REALIZADA";
    }

    public class DashboardViewModel
    {
        public string DiaHoy { get; set; } = string.Empty;

        // Equipo de turno del día, separado por rol.
        public List<TurnoHoyViewModel> Administradores { get; set; } = new();

        public List<TurnoHoyViewModel> Entrenadores { get; set; } = new();

        public List<TurnoHoyViewModel> Empleados { get; set; } = new();

        public string? ErrorEquipo { get; set; }

        // Solo entrenador.
        public List<ClienteAsignadoViewModel> ClientesAsignados { get; set; } = new();

        // Solo empleado.
        public List<KROTraining.Models.HorarioTarea> TareasProximas { get; set; } = new();

        // Solo cliente: su entrenador asignado, si existe.
        public EntrenadorAsignadoViewModel? EntrenadorAsignado { get; set; }
    }

    public class EntrenadorAsignadoViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public string? Especialidad { get; set; }
    }

    public class ClienteAsignadoViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public string? BadgeNumero { get; set; }

        public string? Nivel { get; set; }

        public string? Objetivo { get; set; }
    }
}
