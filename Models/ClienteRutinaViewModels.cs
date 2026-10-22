
using System;
using System.Collections.Generic;

namespace KROTraining.ViewModels
{
    public class ClienteRutinaViewModel
    {
        public int AsignacionId { get; set; }
        public string NombreRutina { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Objetivo { get; set; }
        public string? Nivel { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public int? SesionEnProgresoId { get; set; }
        public List<ClienteRutinaEjercicioViewModel> Ejercicios { get; set; } = new();
    }

    public class ClienteRutinaEjercicioViewModel
    {
        public int EjercicioId { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? GrupoMuscular { get; set; }
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public int DescansoSegundos { get; set; }
        public bool Completado { get; set; }
    }

    public class ClienteSesionViewModel
    {
        public int SesionId { get; set; }
        public string NombreRutina { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "";
        public int TotalEjercicios { get; set; }
        public int EjerciciosCompletados { get; set; }
        public List<ClienteSesionEjercicioViewModel> Ejercicios { get; set; } = new();
    }

    public class ClienteSesionEjercicioViewModel
    {
        public int EjercicioId { get; set; }
        public string Nombre { get; set; } = "";
        public string? GrupoMuscular { get; set; }
        public int? SeriesAsignadas { get; set; }
        public int? RepeticionesAsignadas { get; set; }
        public int? DescansoSegundosAsignado { get; set; }
        public bool Completado { get; set; }
        public DateTime? FechaMarcado { get; set; }
    }

    public class ClienteHistorialItemViewModel
    {
        public int SesionId { get; set; }
        public string NombreRutina { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "";
        public int TotalEjercicios { get; set; }
        public int EjerciciosCompletados { get; set; }
    }
}
