using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Un renglón de "Lo que se entregó": un producto, su sistema de medida
    /// y la cantidad total, dentro de una entrega programada.
    /// </summary>
    public class DetalleEntregaProgramada
    {
        public int Id { get; set; }

        public int EntregaProgramadaId { get; set; }
        public EntregaProgramada? EntregaProgramada { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        [StringLength(100, ErrorMessage = "El producto no puede pasar de 100 caracteres")]
        [Display(Name = "Producto")]
        public string Producto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sistema de medida es obligatorio")]
        [StringLength(30)]
        [Display(Name = "Sistema de medida")]
        public string SistemaMedida { get; set; } = string.Empty;

        [Required(ErrorMessage = "El total es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El total debe ser mayor que 0")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }
    }
}
