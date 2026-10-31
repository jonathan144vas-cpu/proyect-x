using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Empresa a la que le compramos víveres de un área (granos, carnes, lácteos...).
    /// El área usa la misma lista de categorías del inventario, así un pedido de
    /// "Carnes" solo ofrece los proveedores de carnes.
    /// </summary>
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres")]
        [Display(Name = "Nombre del proveedor")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El área es obligatoria")]
        [StringLength(50)]
        [Display(Name = "Área")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede pasar de 20 caracteres")]
        [RegularExpression(@"^[0-9+()\-\s]{8,20}$", ErrorMessage = "Escribe un teléfono válido, por ejemplo 2234-5678")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección de la empresa es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede pasar de 200 caracteres")]
        [Display(Name = "Dirección de la empresa")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe quién es el asesor a cargo")]
        [StringLength(100, ErrorMessage = "El nombre del asesor no puede pasar de 100 caracteres")]
        [Display(Name = "Asesor a cargo")]
        public string AsesorEncargado { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [StringLength(100, ErrorMessage = "El correo no puede pasar de 100 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string? Correo { get; set; }

        [StringLength(400, ErrorMessage = "Las observaciones no pueden pasar de 400 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        public NuevoProveedor? Alta { get; set; }
    }

    /// <summary>
    /// Constancia de cuando se añadió un nuevo proveedor: quién lo registró, cuándo y desde
    /// dónde. Va ligada al proveedor, así su ficha no se llena de datos de auditoría.
    /// </summary>
    public class NuevoProveedor
    {
        public int Id { get; set; }

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        [StringLength(100)]
        [Display(Name = "Registrado por")]
        public string? RegistradoPor { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [StringLength(60)]
        [Display(Name = "Origen")]
        public string Origen { get; set; } = string.Empty;
    }

    /// <summary>Lo mínimo de un proveedor que necesita la lista desplegable del pedido.</summary>
    public record ProveedorOpcion(int Id, string Nombre, string Categoria);
}
