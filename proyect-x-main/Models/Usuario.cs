using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Usuario del sistema. Hereda de IdentityUser (usuario, correo, contraseña, roles)
    /// y le agrega los datos propios de esta aplicación.
    /// </summary>
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede pasar de 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
