using System.ComponentModel.DataAnnotations;

namespace KROTraining.Models
{
    public static class EstadosEvento
    {
        public const string Activo = "ACTIVO";
        public const string Cancelado = "CANCELADO";
        public const string Finalizado = "FINALIZADO";

        public static readonly string[] Todos = { Activo, Cancelado, Finalizado };
    }

    public class EventoListaViewModel
    {
        public int EventoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public int CupoMaximo { get; set; }

        public int Inscritos { get; set; }

        public string Estado { get; set; } = string.Empty;

        public bool EstaInscrito { get; set; }

        // Texto que se muestra en la tabla según estado y fecha.
        public string EtiquetaEstado
        {
            get
            {
                if (Estado == EstadosEvento.Cancelado) return "Cancelado";
                if (Estado == EstadosEvento.Finalizado || Fecha < DateTime.Now) return "Finalizado";
                return "Próximo";
            }
        }
    }

    public class EventoFormViewModel
    {
        public int? EventoId { get; set; }

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres.")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha y hora del evento son obligatorias.")]
        public DateTime? Fecha { get; set; }

        [Range(1, 10000, ErrorMessage = "El cupo debe estar entre 1 y 10000.")]
        public int CupoMaximo { get; set; } = 20;

        public string Estado { get; set; } = EstadosEvento.Activo;

        // Inscripciones actuales, para no dejar el cupo por debajo de lo ya inscrito.
        public int InscritosActuales { get; set; }
    }

    public class EventoDetalleViewModel
    {
        public int EventoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public int CupoMaximo { get; set; }

        public int Inscritos { get; set; }

        public int CuposDisponibles => Math.Max(0, CupoMaximo - Inscritos);

        public string Estado { get; set; } = string.Empty;

        public bool PuedeInscribirse { get; set; }

        public bool YaInscrito { get; set; }

        public bool EsCliente { get; set; }

        public bool PuedeGestionar { get; set; }

        public bool PuedeGestionarInscripciones { get; set; }
    }

    public class InscripcionEventoListaViewModel
    {
        public int EventoId { get; set; }

        public int ClienteId { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string ClienteCorreo { get; set; } = string.Empty;

        public DateTime FechaInscripcion { get; set; }

        public string Estado { get; set; } = string.Empty;
    }

    public class ClienteEventoViewModel
    {
        public int ClienteId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
    }

    public class InscripcionesViewModel
    {
        public int? EventoId { get; set; }

        public string EventoNombre { get; set; } = string.Empty;

        public int CupoMaximo { get; set; }

        public int Inscritos { get; set; }

        public int CuposDisponibles => Math.Max(0, CupoMaximo - Inscritos);

        public List<EventoListaViewModel> Eventos { get; set; } = new();

        public List<InscripcionEventoListaViewModel> Inscripciones { get; set; } = new();

        public List<ClienteEventoViewModel> ClientesDisponibles { get; set; } = new();
    }
}
