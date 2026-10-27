namespace ControlViveresApp.Models
{
    /// <summary>
    /// Porcentaje de población en pobreza total y en pobreza extrema de un departamento,
    /// en la ENCOVI 2023 y en la ENCOVI 2014 (para ver cómo cambió), más su pobreza
    /// multidimensional (IPM-Gt, calculado con la ENCOVI 2023).
    /// </summary>
    public record PobrezaDepartamental(
        string Departamento,
        double PobrezaTotal2023,
        double PobrezaExtrema2023,
        double PobrezaTotal2014,
        double PobrezaExtrema2014,
        double PobrezaMultidimensional2023);

    /// <summary>Un departamento del mapa: sus cifras de pobreza y lo que la campaña ha hecho allí.</summary>
    public record DepartamentoPobreza(
        string Nombre,
        double PobrezaTotal2023,
        double PobrezaExtrema2023,
        double PobrezaTotal2014,
        double PobrezaExtrema2014,
        double PobrezaMultidimensional2023,
        int EntregasRealizadas,
        int FamiliasAtendidas,
        int EnviosPendientes);

    /// <summary>
    /// Cifras de pobreza del Instituto Nacional de Estadística (ENCOVI 2023 y 2014), la
    /// misma fuente que usan los mapas de pobreza de SEGEPLAN, y del informe "Pobreza
    /// multidimensional en Guatemala" (MIDES, INE, SEGEPLAN, MINFIN y PNUD, junio de 2025).
    /// </summary>
    public static class DatosPobreza
    {
        public const double NacionalTotal2023 = 56.0;
        public const double NacionalExtrema2023 = 16.2;
        public const double NacionalTotal2014 = 59.3;
        public const double NacionalExtrema2014 = 23.4;

        // IPM-Gt 2025: porcentaje de personas pobres multidimensionales, qué parte de las
        // privaciones sufren en promedio (intensidad) y la brecha entre el campo y la ciudad.
        public const double NacionalMultidimensional = 57.7;
        public const double IntensidadMultidimensional = 45.1;
        public const double RuralMultidimensional = 76.3;
        public const double UrbanaMultidimensional = 40.8;

        public static readonly PobrezaDepartamental[] PorDepartamento =
        [
            new("Alta Verapaz",    90.3, 58.5, 83.1, 53.6, 87.2),
            new("Baja Verapaz",    80.1, 35.8, 66.3, 24.6, 67.8),
            new("Chimaltenango",   47.8,  5.8, 66.1, 23.4, 53.5),
            new("Chiquimula",      59.6, 25.3, 70.7, 41.1, 69.3),
            new("El Progreso",     46.4,  5.1, 53.2, 13.2, 46.6),
            new("Escuintla",       39.2,  2.6, 52.9, 11.2, 53.3),
            new("Guatemala",       21.6,  1.1, 33.3,  5.4, 25.9),
            new("Huehuetenango",   81.3, 29.7, 73.8, 28.6, 70.3),
            new("Izabal",          50.5, 14.9, 59.9, 35.2, 67.5),
            new("Jalapa",          79.9, 34.5, 67.2, 22.3, 74.1),
            new("Jutiapa",         74.0, 18.8, 62.7, 24.2, 56.7),
            new("Petén",           50.6,  8.4, 60.8, 20.2, 79.8),
            new("Quetzaltenango",  44.1,  2.8, 56.0, 16.7, 44.6),
            new("Quiché",          86.4, 34.4, 74.7, 41.8, 76.5),
            new("Retalhuleu",      50.9,  4.1, 56.1, 15.3, 64.1),
            new("Sacatepéquez",    38.7,  2.5, 41.1,  8.4, 25.3),
            new("San Marcos",      53.7,  6.9, 60.2, 22.0, 67.0),
            new("Santa Rosa",      74.0, 16.2, 54.3, 12.9, 59.2),
            new("Sololá",          61.9,  8.8, 80.9, 40.0, 50.6),
            new("Suchitepéquez",   53.7,  7.7, 63.8, 19.8, 62.2),
            new("Totonicapán",     62.1, 10.1, 77.5, 41.1, 74.3),
            new("Zacapa",          52.6, 13.5, 55.9, 21.4, 59.4)
        ];
    }
}
