namespace ControlViveresApp.Models
{
    /// <summary>Un departamento de Guatemala con su ubicación aproximada.</summary>
    public record Departamento(string Nombre, double Latitud, double Longitud);

    /// <summary>
    /// Listas fijas que usan los formularios y los filtros. Están en un solo lugar
    /// para que no se desincronicen entre las vistas y para poder validarlas en el servidor.
    /// </summary>
    public static class Catalogos
    {
        public static readonly string[] Categorias =
        [
            "Granos",
            "Lácteos",
            "Enlatados",
            "Frutas y Verduras",
            "Carnes",
            "Otros"
        ];

        public static readonly string[] UnidadesMedida =
        [
            "Libras",
            "Kilogramos",
            "Litros",
            "Unidades"
        ];

        /// <summary>
        /// Los 22 departamentos de Guatemala. Las coordenadas son las de su cabecera
        /// y quedan listas para dibujar el mapa más adelante.
        /// </summary>
        public static readonly Departamento[] Departamentos =
        [
            new("Alta Verapaz",    15.4708, -90.3711),
            new("Baja Verapaz",    15.1022, -90.3153),
            new("Chimaltenango",   14.6611, -90.8208),
            new("Chiquimula",      14.8000, -89.5456),
            new("El Progreso",     14.8547, -90.0700),
            new("Escuintla",       14.3050, -90.7850),
            new("Guatemala",       14.6349, -90.5069),
            new("Huehuetenango",   15.3197, -91.4708),
            new("Izabal",          15.7278, -88.5944),
            new("Jalapa",          14.6333, -89.9889),
            new("Jutiapa",         14.2917, -89.8958),
            new("Petén",           16.9280, -89.8942),
            new("Quetzaltenango",  14.8347, -91.5181),
            new("Quiché",          15.0303, -91.1489),
            new("Retalhuleu",      14.5361, -91.6778),
            new("Sacatepéquez",    14.5586, -90.7295),
            new("San Marcos",      14.9639, -91.7944),
            new("Santa Rosa",      14.2769, -90.2981),
            new("Sololá",          14.7719, -91.1831),
            new("Suchitepéquez",   14.5347, -91.5031),
            new("Totonicapán",     14.9114, -91.3611),
            new("Zacapa",          14.9722, -89.5306)
        ];

        public static readonly string[] NombresDepartamentos =
            Departamentos.Select(d => d.Nombre).ToArray();

        public static Departamento? BuscarDepartamento(string? nombre) =>
            Departamentos.FirstOrDefault(d => d.Nombre == nombre);
    }
}
