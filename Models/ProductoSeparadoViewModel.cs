using System.Collections.Generic;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Agrupa los lotes de un mismo producto que están próximos a vencer
    /// (entre 1 y 7 días), para la pantalla de Productos Separados.
    /// </summary>
    public class ProductoSeparadoViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public List<Alimento> Lotes { get; set; } = [];
    }
}
