namespace ControlViveresApp.Models
{
    /// <summary>
    /// Agrupa todos los lotes de un mismo producto para la pantalla de Productos Separados.
    /// </summary>
    public class ProductoSeparadoViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public List<Alimento> Lotes { get; set; } = [];
    }
}
