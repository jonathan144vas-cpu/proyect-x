namespace ControlViveresApp.Models
{
    /// <summary>
    /// Datos de la campaña de recolección. Se configuran en appsettings.json,
    /// así se pueden cambiar sin tocar el código ni la base de datos.
    /// </summary>
    public class OpcionesCampania
    {
        public const string Seccion = "Campania";

        public string Nombre { get; set; } = "Campaña de recolección";

        /// <summary>A dónde se va a entregar lo recaudado.</summary>
        public string Destino { get; set; } = string.Empty;

        /// <summary>Cuántas unidades se quieren juntar en total.</summary>
        public int MetaUnidades { get; set; } = 500;

        /// <summary>Porcentaje alcanzado, tope 100 para que la barra no se desborde.</summary>
        public int PorcentajeDe(int recaudado)
        {
            if (MetaUnidades <= 0) return 0;
            return (int)Math.Min(100, Math.Round(recaudado * 100.0 / MetaUnidades));
        }
    }
}
