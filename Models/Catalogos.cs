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
            "Bolsas",
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

        /// <summary>
        /// Municipios de cada departamento de Guatemala, usados por el desplegable en
        /// cascada de Departamento/Municipio en el módulo de Entregas Programadas.
        /// </summary>
        public static readonly Dictionary<string, string[]> MunicipiosPorDepartamento = new()
        {
            ["Guatemala"] =
            [
                "Guatemala", "Santa Catarina Pinula", "San José Pinula", "San José del Golfo",
                "Palencia", "Chinautla", "San Pedro Ayampuc", "Mixco", "San Pedro Sacatepéquez",
                "San Juan Sacatepéquez", "San Raymundo", "Chuarrancho", "Fraijanes", "Amatitlán",
                "Villa Nueva", "Villa Canales", "San Miguel Petapa"
            ],
            ["El Progreso"] =
            [
                "Guastatoya", "Morazán", "San Agustín Acasaguastlán", "San Cristóbal Acasaguastlán",
                "El Jícaro", "Sansare", "Sanarate", "San Antonio La Paz"
            ],
            ["Sacatepéquez"] =
            [
                "Antigua Guatemala", "Jocotenango", "Pastores", "Sumpango", "Santo Domingo Xenacoj",
                "Santiago Sacatepéquez", "San Bartolomé Milpas Altas", "San Lucas Sacatepéquez",
                "Santa Lucía Milpas Altas", "Magdalena Milpas Altas", "Santa María de Jesús",
                "Ciudad Vieja", "San Miguel Dueñas", "Alotenango", "San Antonio Aguas Calientes",
                "Santa Catarina Barahona"
            ],
            ["Chimaltenango"] =
            [
                "Chimaltenango", "San José Poaquil", "San Martín Jilotepeque", "Comalapa",
                "Santa Apolonia", "Tecpán Guatemala", "Patzún", "Pochuta", "Patzicía",
                "Santa Cruz Balanyá", "Acatenango", "Yepocapa", "San Andrés Itzapa", "Parramos",
                "Zaragoza", "El Tejar"
            ],
            ["Escuintla"] =
            [
                "Escuintla", "Santa Lucía Cotzumalguapa", "La Democracia", "Siquinalá", "Masagua",
                "Tiquisate", "La Gomera", "Guanagazapa", "San José", "Iztapa", "Palín",
                "San Vicente Pacaya", "Nueva Concepción"
            ],
            ["Santa Rosa"] =
            [
                "Cuilapa", "Barberena", "Santa Rosa de Lima", "Casillas", "San Rafael Las Flores",
                "Oratorio", "San Juan Tecuaco", "Chiquimulilla", "Taxisco", "Santa María Ixhuatán",
                "Guazacapán", "Santa Cruz Naranjo", "Pueblo Nuevo Viñas", "Nueva Santa Rosa"
            ],
            ["Sololá"] =
            [
                "Sololá", "San José Chacayá", "Santa María Visitación", "Santa Lucía Utatlán",
                "Nahualá", "Santa Catarina Ixtahuacán", "Santa Clara La Laguna", "Concepción",
                "San Andrés Semetabaj", "Panajachel", "Santa Catarina Palopó", "San Antonio Palopó",
                "San Lucas Tolimán", "Santa Cruz La Laguna", "San Pablo La Laguna",
                "San Marcos La Laguna", "San Juan La Laguna", "San Pedro La Laguna",
                "Santiago Atitlán"
            ],
            ["Totonicapán"] =
            [
                "Totonicapán", "San Cristóbal Totonicapán", "San Francisco El Alto",
                "San Andrés Xecul", "Momostenango", "Santa María Chiquimula",
                "Santa Lucía La Reforma", "San Bartolo"
            ],
            ["Quetzaltenango"] =
            [
                "Quetzaltenango", "Salcajá", "Olintepeque", "San Carlos Sija", "Sibilia", "Cabricán",
                "Cajolá", "San Miguel Sigüilá", "Ostuncalco", "San Mateo", "Concepción Chiquirichapa",
                "San Martín Sacatepéquez", "Almolonga", "Cantel", "Huitán", "Zunil", "Colomba",
                "San Francisco La Unión", "El Palmar", "Coatepeque", "Génova", "Flores Costa Cuca",
                "La Esperanza", "Palestina de Los Altos"
            ],
            ["Suchitepéquez"] =
            [
                "Mazatenango", "Cuyotenango", "San Francisco Zapotitlán", "San Bernardino",
                "San José El Ídolo", "Santo Domingo Suchitepéquez", "San Lorenzo", "Samayac",
                "San Pablo Jocopilas", "San Antonio Suchitepéquez", "San Miguel Panán", "San Gabriel",
                "Chicacao", "Patulul", "Santa Bárbara", "San Juan Bautista", "Santo Tomás La Unión",
                "Zunilito", "Pueblo Nuevo", "Río Bravo", "San José La Máquina"
            ],
            ["Retalhuleu"] =
            [
                "Retalhuleu", "San Sebastián", "Santa Cruz Muluá", "San Martín Zapotitlán",
                "San Felipe", "San Andrés Villa Seca", "Champerico", "Nuevo San Carlos", "El Asintal"
            ],
            ["San Marcos"] =
            [
                "San Marcos", "San Pedro Sacatepéquez", "San Antonio Sacatepéquez", "Comitancillo",
                "San Miguel Ixtahuacán", "Concepción Tutuapa", "Tacaná", "Sibinal", "Tajumulco",
                "Tejutla", "San Rafael Pie de la Cuesta", "Nuevo Progreso", "El Tumbador", "El Rodeo",
                "Malacatán", "Catarina", "Ayutla", "Ocós", "San Pablo", "El Quetzal", "La Reforma",
                "Pajapita", "Ixchiguán", "San José Ojetenam", "San Cristóbal Cucho", "Sipacapa",
                "Esquipulas Palo Gordo", "Río Blanco", "San Lorenzo", "La Blanca"
            ],
            ["Huehuetenango"] =
            [
                "Huehuetenango", "Chiantla", "Malacatancito", "Cuilco", "Nentón", "San Pedro Necta",
                "Jacaltenango", "Soloma", "Ixtahuacán", "Santa Bárbara", "La Libertad", "La Democracia",
                "San Miguel Acatán", "San Rafael La Independencia", "Todos Santos Cuchumatán",
                "San Juan Atitán", "Santa Eulalia", "San Mateo Ixtatán", "Colotenango",
                "San Sebastián Huehuetenango", "Tectitán", "Concepción Huista", "San Juan Ixcoy",
                "San Antonio Huista", "San Sebastián Coatán", "Barillas", "Aguacatán",
                "San Rafael Petzal", "San Gaspar Ixchil", "Santiago Chimaltenango",
                "Santa Ana Huista", "Unión Cantinil"
            ],
            ["Quiché"] =
            [
                "Santa Cruz del Quiché", "Chiché", "Chinique", "Zacualpa", "Chajul",
                "Chichicastenango", "Patzité", "San Antonio Ilotenango", "San Pedro Jocopilas",
                "Cunén", "San Juan Cotzal", "Joyabaj", "Nebaj", "San Andrés Sajcabajá", "Uspantán",
                "Sacapulas", "San Bartolomé Jocotenango", "Canillá", "Chicamán", "Ixcán", "Pachalum"
            ],
            ["Baja Verapaz"] =
            [
                "Salamá", "San Miguel Chicaj", "Rabinal", "Cubulco", "Granados", "Santa Cruz El Chol",
                "San Jerónimo", "Purulhá"
            ],
            ["Alta Verapaz"] =
            [
                "Cobán", "Santa Cruz Verapaz", "San Cristóbal Verapaz", "Tactic", "Tamahú", "Tucurú",
                "Panzós", "Senahú", "San Pedro Carchá", "San Juan Chamelco", "Lanquín", "Cahabón",
                "Chisec", "Chahal", "Fray Bartolomé de las Casas", "Santa Catalina La Tinta",
                "Raxruhá"
            ],
            ["Petén"] =
            [
                "Flores", "San José", "San Benito", "San Andrés", "La Libertad", "San Francisco",
                "Santa Ana", "Dolores", "San Luis", "Sayaxché", "Melchor de Mencos", "Poptún",
                "Las Cruces", "El Chal"
            ],
            ["Izabal"] =
            [
                "Puerto Barrios", "Livingston", "El Estor", "Morales", "Los Amates"
            ],
            ["Zacapa"] =
            [
                "Zacapa", "Estanzuela", "Río Hondo", "Gualán", "Teculután", "Usumatlán", "Cabañas",
                "San Diego", "La Unión", "Huité"
            ],
            ["Chiquimula"] =
            [
                "Chiquimula", "San José La Arada", "San Juan Ermita", "Jocotán", "Camotán", "Olopa",
                "Esquipulas", "Concepción Las Minas", "Quezaltepeque", "San Jacinto", "Ipala"
            ],
            ["Jalapa"] =
            [
                "Jalapa", "San Pedro Pinula", "San Luis Jilotepeque", "San Manuel Chaparrón",
                "San Carlos Alzatate", "Monjas", "Mataquescuintla"
            ],
            ["Jutiapa"] =
            [
                "Jutiapa", "El Progreso", "Santa Catarina Mita", "Agua Blanca", "Asunción Mita",
                "Yupiltepeque", "Atescatempa", "Jerez", "San José Acatempa", "Comapa", "Jalpatagua",
                "Conguaco", "Moyuta", "Pasaco", "San Adrián", "Zapotitlán", "Quesada"
            ]
        };

        public static string[] MunicipiosDe(string? departamento) =>
            !string.IsNullOrWhiteSpace(departamento) &&
            MunicipiosPorDepartamento.TryGetValue(departamento, out var municipios)
                ? municipios
                : [];
    }
}
