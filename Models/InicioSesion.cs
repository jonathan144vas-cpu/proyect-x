using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Datos del formulario de inicio de sesión.
    /// </summary>
    public class InicioSesion
    {
        [Required(ErrorMessage = "Escribe tu usuario")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe tu contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Display(Name = "Mantener la sesión iniciada")]
        public bool Recordarme { get; set; }
    }
}
