namespace ControlViveresApp.Models
{
    /// <summary>Un valor con su etiqueta: una rebanada del pastel o un punto de la línea.</summary>
    public record Punto(string Etiqueta, int Valor);

    /// <summary>
    /// Todo lo que necesita la pantalla "Visualización": cifras sueltas para las tarjetas
    /// y las series de cada gráfica. Se calcula en el controlador y la vista solo lo dibuja.
    /// </summary>
    public class VisualizacionViewModel
    {
        // Tarjetas de resumen
        public int UnidadesEnBodega { get; set; }
        public int ProductosDistintos { get; set; }
        public int UnidadesPorVencer { get; set; }
        public int EnviosPorHacer { get; set; }
        public int FamiliasPorAtender { get; set; }
        public int PedidosActivos { get; set; }

        // Gráficas de pastel
        public List<Punto> InventarioPorCategoria { get; set; } = [];
        public List<Punto> EstadoVencimiento { get; set; } = [];
        public List<Punto> EnviosPorEstado { get; set; } = [];
        public List<Punto> EnviosPendientesPorDepartamento { get; set; } = [];
        public List<Punto> PedidosPorEstado { get; set; } = [];
        public List<Punto> PedidosActivosPorPrioridad { get; set; } = [];

        // Gráficas de línea: un punto por mes ("yyyy-MM")
        public List<Punto> EntregasRealizadasPorMes { get; set; } = [];
        public List<Punto> EntregasProgramadasPorMes { get; set; } = [];
        public List<Punto> IngresoInventarioPorMes { get; set; } = [];
    }
}
