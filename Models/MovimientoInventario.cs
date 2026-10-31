using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    public enum TipoMovimiento
    {
        Entrada = 0,
        Salida = 1
    }

    /// <summary>
    /// Cada vez que entran o salen víveres de la bodega queda un renglón aquí: un alta en
    /// el inventario, un pedido recibido o una entrega completada. Las gráficas de
    /// Visualización se arman con esto, así siempre reflejan lo que realmente entró y salió,
    /// aunque después se sumen cantidades a un lote o se descuenten por una entrega.
    /// </summary>
    public class MovimientoInventario
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public TipoMovimiento Tipo { get; set; }

        [Required]
        [StringLength(100)]
        public string Producto { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        [StringLength(30)]
        public string UnidadMedida { get; set; } = string.Empty;

        /// <summary>De dónde viene el movimiento, por ejemplo "Pedido #12" o "Entrega a Comunidad Mixco".</summary>
        [StringLength(150)]
        public string Origen { get; set; } = string.Empty;

        [StringLength(100)]
        public string? RegistradoPor { get; set; }

        public static MovimientoInventario Nuevo(
            TipoMovimiento tipo, string producto, string categoria, int cantidad,
            string unidadMedida, string origen, string? usuario) => new()
        {
            Fecha = DateTime.UtcNow,
            Tipo = tipo,
            Producto = producto,
            Categoria = categoria,
            Cantidad = cantidad,
            UnidadMedida = unidadMedida,
            Origen = origen,
            RegistradoPor = usuario
        };
    }
}
