using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class ConfigurarPermisosViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador del rol no es válido.")]
        public int RolId { get; set; }

        // Información que cargaremos desde la base de datos.
        [BindNever]
        [ValidateNever]
        public string NombreRol { get; set; } = string.Empty;

        // Identificadores de las casillas seleccionadas.
        public List<int> PermisosSeleccionados { get; set; }
            = new List<int>();

        // Catálogo que mostraremos en el formulario.
        [BindNever]
        [ValidateNever]
        public List<PermisoOpcionViewModel> PermisosDisponibles
        { get; set; } = new List<PermisoOpcionViewModel>();
    }

    public class PermisoOpcionViewModel
    {
        public int PermisoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }
}