namespace KROTraining.Models
{
    public class RestablecerPasswordViewModel
    {
        public string Correo { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}