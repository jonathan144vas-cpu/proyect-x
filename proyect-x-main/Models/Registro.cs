using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Datos del formulario público de creación de cuenta.
    /// Las cuentas creadas aquí reciben el rol "Usuario" (solo lectura);
    /// un administrador puede ascenderlas más adelante.
    /// </summary>
    public class Registro
    {
        [Required(ErrorMessage = "Escribe tu nombre completo")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede pasar de 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe tu correo")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe un nombre de usuario")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe una contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
