using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KROTraining.Models
{
    public class EliminarRutinaViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "El identificador de la rutina no es válido.")]
        public int RutinaId { get; set; }

        [BindNever]
        [ValidateNever]
        public string Nombre { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public string? Objetivo { get; set; }

        [BindNever]
        [ValidateNever]
        public string? Nivel { get; set; }

        [BindNever]
        [ValidateNever]
        public int CantidadEjercicios { get; set; }

        [BindNever]
        [ValidateNever]
        public bool TieneAsignaciones { get; set; }

        [BindNever]
        [ValidateNever]
        public bool TieneRecomendaciones { get; set; }

        [BindNever]
        [ValidateNever]
        public bool PuedeEliminar =>
            !TieneAsignaciones && !TieneRecomendaciones;

        [Range(typeof(bool), "true", "true",
            ErrorMessage = "Debe confirmar la eliminación para continuar.")]
        [Display(Name = "Confirmo que deseo eliminar esta rutina.")]
        public bool ConfirmarEliminacion { get; set; }
    }
}
