using ControlViveresApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Data
{
    /// <summary>
    /// Aplica las migraciones pendientes y siembra los roles, usuarios y datos
    /// iniciales de demostración. Se ejecuta una sola vez, al arrancar la aplicación;
    /// cada siembra revisa primero si la tabla ya tiene datos para no duplicar.
    /// </summary>
    public static class InicializadorBaseDatos
    {
        // Semilla fija: los mismos datos de ejemplo se generan siempre igual,
        // sin importar cuántas veces se reinicie la aplicación.
        private static readonly Random Aleatorio = new(2026);

        public static async Task InicializarAsync(IServiceProvider servicios)
        {
            var registrador = servicios.GetRequiredService<ILoggerFactory>()
                                       .CreateLogger(nameof(InicializadorBaseDatos));

            var contexto = servicios.GetRequiredService<ContextoViveres>();
            var gestorRoles = servicios.GetRequiredService<RoleManager<IdentityRole>>();
            var gestorUsuarios = servicios.GetRequiredService<UserManager<Usuario>>();

            // 1. Crear la base de datos y aplicar migraciones pendientes.
            await contexto.Database.MigrateAsync();

            // 2. Crear los roles si no existen.
            foreach (var rol in new[] { "Admin", "Usuario" })
            {
                if (!await gestorRoles.RoleExistsAsync(rol))
                {
                    await gestorRoles.CreateAsync(new IdentityRole(rol));
                }
            }

            // 3. Crear los usuarios de arranque.
            //    OJO: son credenciales de prueba visibles en el código. Antes de publicar
            //    esto en un servidor real, muévelas a configuración o secretos de usuario.
            await CrearUsuarioAsync(
                gestorUsuarios, registrador,
                nombreUsuario: "admin",
                correo: "admin@viveres.com",
                nombreCompleto: "Administrador Principal",
                contrasena: "Admin123*",
                rol: "Admin");

            await CrearUsuarioAsync(
                gestorUsuarios, registrador,
                nombreUsuario: "lector",
                correo: "lector@viveres.com",
                nombreCompleto: "Usuario Solo Lectura",
                contrasena: "Lector123*",
                rol: "Usuario");

            // 4. Sembrar los datos de demostración (inventario, proveedores, pedidos e historial).
            await SembrarAlimentosAsync(contexto, registrador);
            await SembrarProveedoresAsync(contexto, registrador);
            await SembrarPedidosAsync(contexto, registrador);
            await SembrarEntregasAsync(contexto, registrador);
            await SembrarEntregasProgramadasAsync(contexto, registrador);
            await SembrarVisitasPreviasAsync(contexto, registrador);
        }

        private static async Task CrearUsuarioAsync(
            UserManager<Usuario> gestorUsuarios,
            ILogger registrador,
            string nombreUsuario,
            string correo,
            string nombreCompleto,
            string contrasena,
            string rol)
        {
            if (await gestorUsuarios.FindByNameAsync(nombreUsuario) is not null)
            {
                return;
            }

            var usuario = new Usuario
            {
                UserName = nombreUsuario,
                Email = correo,
                NombreCompleto = nombreCompleto,
                EmailConfirmed = true
            };

            var resultado = await gestorUsuarios.CreateAsync(usuario, contrasena);

            if (resultado.Succeeded)
            {
                await gestorUsuarios.AddToRoleAsync(usuario, rol);
                registrador.LogInformation("Usuario inicial '{Usuario}' creado con el rol '{Rol}'.", nombreUsuario, rol);
            }
            else
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                registrador.LogError("No se pudo crear el usuario inicial '{Usuario}': {Errores}", nombreUsuario, errores);
            }
        }

        // Nombre, categoría, unidad de medida y si es un producto no perecedero
        // (sin fecha de vencimiento, como artículos de higiene).
        private static readonly (string Nombre, string Categoria, string Unidad, bool NoPerecedero)[] Productos =
        [
            ("Arroz", "Granos", "Libras", false),
            ("Frijol Negro", "Granos", "Libras", false),
            ("Frijol Rojo", "Granos", "Libras", false),
            ("Maíz", "Granos", "Libras", false),
            ("Avena", "Granos", "Libras", false),
            ("Fideos", "Granos", "Libras", false),
            ("Harina de Maíz", "Granos", "Libras", false),
            ("Harina de Trigo", "Granos", "Libras", false),
            ("Lentejas", "Granos", "Libras", false),
            ("Arvejas", "Granos", "Libras", false),
            ("Leche en Polvo", "Lácteos", "Libras", false),
            ("Leche Fluida", "Lácteos", "Litros", false),
            ("Queso Fresco", "Lácteos", "Libras", false),
            ("Yogurt", "Lácteos", "Litros", false),
            ("Mantequilla", "Lácteos", "Libras", false),
            ("Crema", "Lácteos", "Litros", false),
            ("Incaparina", "Lácteos", "Libras", false),
            ("Atún Enlatado", "Enlatados", "Unidades", false),
            ("Sardinas Enlatadas", "Enlatados", "Unidades", false),
            ("Frijol Enlatado", "Enlatados", "Unidades", false),
            ("Maíz Enlatado", "Enlatados", "Unidades", false),
            ("Chile Enlatado", "Enlatados", "Unidades", false),
            ("Duraznos Enlatados", "Enlatados", "Unidades", false),
            ("Leche Condensada", "Enlatados", "Unidades", false),
            ("Banano", "Frutas y Verduras", "Libras", false),
            ("Plátano", "Frutas y Verduras", "Libras", false),
            ("Papa", "Frutas y Verduras", "Libras", false),
            ("Cebolla", "Frutas y Verduras", "Libras", false),
            ("Tomate", "Frutas y Verduras", "Libras", false),
            ("Zanahoria", "Frutas y Verduras", "Libras", false),
            ("Repollo", "Frutas y Verduras", "Kilogramos", false),
            ("Naranja", "Frutas y Verduras", "Libras", false),
            ("Aguacate", "Frutas y Verduras", "Unidades", false),
            ("Güisquil", "Frutas y Verduras", "Libras", false),
            ("Pollo", "Carnes", "Libras", false),
            ("Carne de Res", "Carnes", "Libras", false),
            ("Carne de Cerdo", "Carnes", "Libras", false),
            ("Huevos", "Carnes", "Unidades", false),
            ("Embutidos", "Carnes", "Libras", false),
            ("Pescado", "Carnes", "Libras", false),
            ("Sal", "Otros", "Libras", true),
            ("Azúcar", "Otros", "Libras", true),
            ("Aceite", "Otros", "Litros", true),
            ("Café", "Otros", "Libras", true),
            ("Jabón de Baño", "Otros", "Unidades", true),
            ("Papel Higiénico", "Otros", "Bolsas", true),
            ("Pasta Dental", "Otros", "Unidades", true),
            ("Cepillo Dental", "Otros", "Unidades", true),
            ("Toallas Sanitarias", "Otros", "Bolsas", true),
            ("Pañales", "Otros", "Bolsas", true),

            ("Garbanzos", "Granos", "Libras", false),
            ("Frijol Blanco", "Granos", "Libras", false),
            ("Arroz Integral", "Granos", "Libras", false),
            ("Cereal de Maíz", "Granos", "Bolsas", false),
            ("Galletas Integrales", "Granos", "Bolsas", false),
            ("Pan de Caja", "Granos", "Bolsas", false),
            ("Pasta Corta", "Granos", "Libras", false),
            ("Queso Seco", "Lácteos", "Libras", false),
            ("Leche Descremada", "Lácteos", "Litros", false),
            ("Leche de Soya", "Lácteos", "Litros", false),
            ("Requesón", "Lácteos", "Libras", false),
            ("Leche Evaporada", "Lácteos", "Unidades", false),
            ("Sopa Enlatada", "Enlatados", "Unidades", false),
            ("Salsa de Tomate", "Enlatados", "Unidades", false),
            ("Garbanzos Enlatados", "Enlatados", "Unidades", false),
            ("Champiñones Enlatados", "Enlatados", "Unidades", false),
            ("Piña Enlatada", "Enlatados", "Unidades", false),
            ("Jamón Enlatado", "Enlatados", "Unidades", false),
            ("Papaya", "Frutas y Verduras", "Libras", false),
            ("Piña", "Frutas y Verduras", "Unidades", false),
            ("Limón", "Frutas y Verduras", "Libras", false),
            ("Mango", "Frutas y Verduras", "Libras", false),
            ("Pepino", "Frutas y Verduras", "Libras", false),
            ("Brócoli", "Frutas y Verduras", "Libras", false),
            ("Lechuga", "Frutas y Verduras", "Unidades", false),
            ("Ajo", "Frutas y Verduras", "Libras", false),
            ("Chile Pimiento", "Frutas y Verduras", "Libras", false),
            ("Ejote", "Frutas y Verduras", "Libras", false),
            ("Chorizo", "Carnes", "Libras", false),
            ("Pechuga de Pollo", "Carnes", "Libras", false),
            ("Carne Molida", "Carnes", "Libras", false),
            ("Costillas de Cerdo", "Carnes", "Libras", false),
            ("Camarón", "Carnes", "Libras", false),
            ("Miel de Abeja", "Otros", "Unidades", true),
            ("Mermelada", "Otros", "Unidades", true),
            ("Consomé en Polvo", "Otros", "Bolsas", true),
            ("Detergente en Polvo", "Otros", "Bolsas", true),
            ("Jabón de Lavar", "Otros", "Unidades", true),
            ("Cloro", "Otros", "Litros", true),
            ("Shampoo", "Otros", "Unidades", true)
        ];

        // Cuántos días de vida útil tiene un lote nuevo, según el tipo de producto.
        private static (int Minimo, int Maximo) DiasDeVida(string categoria) => categoria switch
        {
            "Frutas y Verduras" => (4, 21),
            "Carnes" => (5, 30),
            "Lácteos" => (8, 60),
            _ => (90, 540)
        };

        private static async Task SembrarAlimentosAsync(ContextoViveres contexto, ILogger registrador)
        {
            // No se sobreescribe ni se duplica lo que ya exista: se completa hasta tener
            // el catálogo de demostración completo, saltando los nombres ya presentes.
            var nombresExistentes = (await contexto.Alimentos.Select(a => a.Nombre).ToListAsync())
                .Select(n => n.Trim().ToLower())
                .ToHashSet();

            var alimentos = new List<Alimento>();

            foreach (var producto in Productos)
            {
                if (nombresExistentes.Contains(producto.Nombre.ToLower()))
                {
                    continue;
                }

                // Cantidades entre 300 y 1500 por producto.
                var cantidad = Aleatorio.Next(300, 1500);

                DateOnly? vencimiento = null;
                if (!producto.NoPerecedero)
                {
                    // Lotes recién llegados: la vida útil depende del tipo de producto, así
                    // que los frescos quedan pronto por vencer y los secos duran meses.
                    var (minimo, maximo) = DiasDeVida(producto.Categoria);
                    vencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(Aleatorio.Next(minimo, maximo + 1)));
                }

                alimentos.Add(new Alimento
                {
                    Nombre = producto.Nombre,
                    Categoria = producto.Categoria,
                    Cantidad = cantidad,
                    UnidadMedida = producto.Unidad,
                    FechaVencimiento = vencimiento,
                    FechaRegistro = DateTime.UtcNow.AddDays(-Aleatorio.Next(1, 120))
                });
            }

            if (alimentos.Count == 0)
            {
                return;
            }

            contexto.Alimentos.AddRange(alimentos);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} productos de inventario.", alimentos.Count);
        }

        // Cinco proveedores de ejemplo por cada área del inventario.
        // (Área, nombre, teléfono, dirección, asesor a cargo, correo)
        private static readonly (string Area, string Nombre, string Telefono, string Direccion, string Asesor, string Correo)[] ProveedoresDeEjemplo =
        [
            ("Granos", "Granos del Altiplano", "2251-4010", "3a. avenida 12-45, zona 4, Ciudad de Guatemala", "Marta Lucía Ordóñez", "ventas@granosaltiplano.example"),
            ("Granos", "Molinos San Rafael", "2360-7782", "Km 18.5 carretera al Atlántico, Guatemala", "Julio Ernesto Barrios", "pedidos@molinossanrafael.example"),
            ("Granos", "Distribuidora El Sol", "7761-2290", "5a. calle 8-20, zona 1, Quetzaltenango", "Karla Beatriz Monzón", "contacto@distelsol.example"),
            ("Granos", "Cooperativa Agrícola Cuchumatán", "7764-5518", "Barrio El Calvario, Huehuetenango", "Rubén Aroldo Mérida", "cooperativa@cuchumatan.example"),
            ("Granos", "Abarrotes La Esperanza", "2232-9047", "6a. avenida 15-30, zona 1, Ciudad de Guatemala", "Silvia Patricia Gómez", "ventas@abarroteslaesperanza.example"),

            ("Lácteos", "Lácteos La Montaña", "7951-3376", "Km 210 carretera a Cobán, Alta Verapaz", "Carlos Humberto Choc", "ventas@lacteoslamontana.example"),
            ("Lácteos", "Productos Lácteos Santa Elena", "2440-1865", "Calzada Roosevelt 24-18, zona 11, Ciudad de Guatemala", "Ana Gabriela Fuentes", "pedidos@santaelena.example"),
            ("Lácteos", "Cooperativa Ganadera del Norte", "7942-6603", "Avenida Central 4-12, Salamá, Baja Verapaz", "Oswaldo René Tzoc", "ganaderos@delnorte.example"),
            ("Lácteos", "Distribuidora Fresca Leche", "2331-7704", "12 calle 5-60, zona 3, Mixco", "Brenda Mishell Pérez", "contacto@frescaleche.example"),
            ("Lácteos", "Quesos y Cremas Atitlán", "7762-0941", "Calle del Lago 2-15, Sololá", "Domingo Tuy", "quesos@atitlan.example"),

            ("Enlatados", "Enlatados Pacífico", "7880-2217", "Km 92 carretera a Escuintla, Escuintla", "Héctor Manuel Villatoro", "ventas@enlatadospacifico.example"),
            ("Enlatados", "Importadora Marina GT", "2288-3450", "Boulevard Liberación 8-30, zona 9, Ciudad de Guatemala", "Lucrecia Andrea Solís", "importaciones@marinagt.example"),
            ("Enlatados", "Conservas Don Chepe", "2471-6029", "7a. avenida 3-44, zona 6, Ciudad de Guatemala", "José Alfredo Cifuentes", "donchepe@conservas.example"),
            ("Enlatados", "Alimentos Selectos", "2366-5801", "Diagonal 6, 10-65, zona 10, Ciudad de Guatemala", "Paola Fernanda Rivera", "ventas@alimentosselectos.example"),
            ("Enlatados", "Central de Enlatados Zona 4", "2334-9916", "4a. avenida 9-12, zona 4, Ciudad de Guatemala", "Edwin Estuardo López", "central@enlatadoszona4.example"),

            ("Frutas y Verduras", "Agroinsumos GT", "7839-4472", "Aldea San Jerónimo, Tecpán Guatemala, Chimaltenango", "Verónica Isabel Cux", "agro@insumosgt.example"),
            ("Frutas y Verduras", "Hortalizas Tecpán", "7840-1188", "Km 87 carretera Interamericana, Tecpán, Chimaltenango", "Mauricio Sacbajá", "ventas@hortalizastecpan.example"),
            ("Frutas y Verduras", "Frutas del Valle", "5512-3390", "Mercado Central, local 14, Antigua Guatemala", "Rosa Elena Batz", "frutas@delvalle.example"),
            ("Frutas y Verduras", "Mercado La Terminal Mayorista", "2253-8845", "Zona 4, Terminal de Buses, Ciudad de Guatemala", "Gerson Alexander Pop", "mayorista@laterminal.example"),
            ("Frutas y Verduras", "Cooperativa Rural Sacatepéquez", "7832-5567", "Calle Real 6-02, San Lucas Sacatepéquez", "Dina Marisol Ajú", "rural@cooperativasac.example"),

            ("Carnes", "Carnes Premium Escuintla", "7888-6134", "Boulevard Los Próceres 3-50, Escuintla", "Ramiro Antonio Esquivel", "ventas@carnespremium.example"),
            ("Carnes", "Avícola San José", "6637-2209", "Km 44 carretera a Santa Rosa, Cuilapa", "Wendy Carolina Orellana", "pedidos@avicolasanjose.example"),
            ("Carnes", "Embutidos La Selecta", "2476-3318", "20 calle 12-40, zona 12, Ciudad de Guatemala", "Fredy Leonel Castañeda", "contacto@embutidosselecta.example"),
            ("Carnes", "Pescados y Mariscos del Pacífico", "7885-9702", "Muelle Pesquero, Puerto San José, Escuintla", "Ingrid Johanna Santos", "mariscos@delpacifico.example"),
            ("Carnes", "Rastro Municipal Distribución", "7761-8830", "Final 4a. avenida, zona 3, Quetzaltenango", "Byron Obdulio Tahay", "rastro@distribucion.example"),

            ("Otros", "Almacenes La Económica", "2253-4471", "9a. calle 6-18, zona 1, Ciudad de Guatemala", "Claudia Marisol Aguilar", "ventas@laeconomica.example"),
            ("Otros", "Higiene y Hogar GT", "2368-0925", "Avenida Petapa 40-12, zona 12, Ciudad de Guatemala", "Pablo Estuardo Girón", "pedidos@higienehogar.example"),
            ("Otros", "Central de Abastos", "2219-7743", "Calzada Aguilar Batres 33-05, zona 11, Ciudad de Guatemala", "Nancy Yesenia Morales", "central@abastos.example"),
            ("Otros", "Distribuidora San José", "7766-1058", "Calle Rodolfo Robles 10-22, Quetzaltenango", "Otto René Sandoval", "ventas@distsanjose.example"),
            ("Otros", "Papelera Centroamericana", "2325-6694", "Km 15.5 carretera a El Salvador, Santa Catarina Pinula", "Mónica Alejandra Reyes", "papelera@centroamericana.example")
        ];

        private static async Task SembrarProveedoresAsync(ContextoViveres contexto, ILogger registrador)
        {
            // Se completa cada área por separado, así lo que ya exista no se duplica.
            var existentes = (await contexto.Proveedores.Select(p => new { p.Categoria, p.Nombre }).ToListAsync())
                .Select(p => (p.Categoria, p.Nombre.ToLower()))
                .ToHashSet();

            var nuevos = new List<Proveedor>();

            foreach (var ejemplo in ProveedoresDeEjemplo)
            {
                if (existentes.Contains((ejemplo.Area, ejemplo.Nombre.ToLower())))
                {
                    continue;
                }

                nuevos.Add(new Proveedor
                {
                    Nombre = ejemplo.Nombre,
                    Categoria = ejemplo.Area,
                    Telefono = ejemplo.Telefono,
                    Direccion = ejemplo.Direccion,
                    AsesorEncargado = ejemplo.Asesor,
                    Correo = ejemplo.Correo,
                    Alta = new NuevoProveedor
                    {
                        RegistradoPor = "admin",
                        FechaRegistro = DateTime.UtcNow,
                        Origen = "Datos de demostración"
                    }
                });
            }

            if (nuevos.Count == 0)
            {
                return;
            }

            contexto.Proveedores.AddRange(nuevos);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} proveedores.", nuevos.Count);
        }

        private const int MetaPedidosSembrados = 40;

        private static async Task SembrarPedidosAsync(ContextoViveres contexto, ILogger registrador)
        {
            var existentes = await contexto.Pedidos.CountAsync();
            var faltantes = MetaPedidosSembrados - existentes;

            if (faltantes <= 0)
            {
                return;
            }

            var proveedoresPorArea = (await contexto.Proveedores.AsNoTracking().ToListAsync())
                .GroupBy(p => p.Categoria)
                .ToDictionary(g => g.Key, g => g.ToList());

            var prioridades = new[] { PrioridadPedido.Baja, PrioridadPedido.Normal, PrioridadPedido.Alta };
            var estados = new[] { EstadoPedido.Pendiente, EstadoPedido.Solicitado };
            var pedidos = new List<Pedido>();

            for (var i = 0; i < faltantes; i++)
            {
                var producto = Productos[i % Productos.Length];

                Proveedor? proveedor = proveedoresPorArea.TryGetValue(producto.Categoria, out var delArea)
                    ? delArea[Aleatorio.Next(delArea.Count)]
                    : null;

                pedidos.Add(new Pedido
                {
                    Articulo = producto.Nombre,
                    Categoria = producto.Categoria,
                    Cantidad = Aleatorio.Next(50, 500),
                    UnidadMedida = producto.Unidad,
                    ProveedorId = proveedor?.Id,
                    Proveedor = proveedor?.Nombre,
                    FechaNecesaria = DateOnly.FromDateTime(DateTime.Now.AddDays(Aleatorio.Next(3, 45))),
                    // Casi todos los pedidos por realizar no tienen aún un lote definido;
                    // solo se define la fecha de vencimiento hasta que el lote llega a bodega.
                    FechaVencimiento = null,
                    Prioridad = prioridades[Aleatorio.Next(prioridades.Length)],
                    Estado = estados[Aleatorio.Next(estados.Length)],
                    FechaSolicitud = DateTime.UtcNow.AddDays(-Aleatorio.Next(1, 60)),
                    SolicitadoPor = "admin"
                });
            }

            contexto.Pedidos.AddRange(pedidos);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} pedidos por realizar.", pedidos.Count);
        }

        private const int MetaEntregasSembradas = 50;

        // Postgres exige Kind=Utc para "timestamp with time zone"; DateOnly.ToDateTime
        // siempre da Kind=Unspecified, así que hay que forzarlo antes de guardar.
        private static DateTime AFechaUtc(DateOnly fecha) =>
            DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        /// <summary>Entre 1 y 3 productos distintos, con cantidades de ejemplo.</summary>
        private static List<(string Producto, string Unidad, int Total)> GenerarProductosSupuestos()
        {
            var cantidadProductos = Aleatorio.Next(1, 4);

            return Enumerable.Range(0, cantidadProductos)
                .Select(_ => Productos[Aleatorio.Next(Productos.Length)])
                .GroupBy(p => p.Nombre)
                .Select(g => g.First())
                .Select(producto => (producto.Nombre, producto.Unidad, Aleatorio.Next(20, 300)))
                .ToList();
        }

        /// <summary>
        /// Siembra el historial de "Donaciones entregadas". Como son entregas que ya
        /// pasaron y los víveres ya no están en bodega, cada una lleva su propia Entrega
        /// Programada "Completada" con productos supuestos (no hace falta que existan
        /// en el inventario actual): eso le da a cada fila su detalle de productos y su
        /// comprobante en PDF, igual que a las entregas completadas desde la aplicación.
        /// </summary>
        private static async Task SembrarEntregasAsync(ContextoViveres contexto, ILogger registrador)
        {
            var existentes = await contexto.Entregas.CountAsync();
            var faltantes = MetaEntregasSembradas - existentes;

            if (faltantes <= 0)
            {
                return;
            }

            // Se recorren los municipios de todos los departamentos para que el historial
            // quede repartido entre distintos municipios y departamentos de Guatemala.
            var municipiosConDepartamento = Catalogos.MunicipiosPorDepartamento
                .SelectMany(par => par.Value.Select(municipio => (Departamento: par.Key, Municipio: municipio)))
                .ToArray();

            var entregasProgramadas = new List<EntregaProgramada>();
            var entregas = new List<Entrega>();

            for (var i = 0; i < faltantes; i++)
            {
                var (departamento, municipio) = municipiosConDepartamento[
                    (int)((long)i * municipiosConDepartamento.Length / faltantes) % municipiosConDepartamento.Length];

                var fechaEntrega = DateOnly.FromDateTime(DateTime.Now.AddDays(-Aleatorio.Next(1, 365)));
                var familias = Aleatorio.Next(15, 160);
                var productos = GenerarProductosSupuestos();

                var entregaProgramada = new EntregaProgramada
                {
                    Lugar = $"Comunidad {municipio}",
                    Departamento = departamento,
                    Municipio = municipio,
                    FechaProgramada = fechaEntrega,
                    FamiliasBeneficiadas = familias,
                    Estado = EstadoEntregaProgramada.Completada,
                    RegistradoPor = "admin",
                    FechaRegistro = AFechaUtc(fechaEntrega).AddDays(-Aleatorio.Next(1, 10)),
                    FechaCompletada = AFechaUtc(fechaEntrega),
                    Detalles = productos.Select(p => new DetalleEntregaProgramada
                    {
                        Producto = p.Producto,
                        SistemaMedida = p.Unidad,
                        Total = p.Total
                    }).ToList()
                };
                entregasProgramadas.Add(entregaProgramada);

                entregas.Add(new Entrega
                {
                    Lugar = entregaProgramada.Lugar,
                    Departamento = departamento,
                    Municipio = municipio,
                    FechaEntrega = fechaEntrega,
                    FamiliasBeneficiadas = familias,
                    TotalEntregado = productos.Sum(p => p.Total),
                    RegistradoPor = "admin",
                    FechaRegistro = entregaProgramada.FechaRegistro,
                    // La navegación (en vez del Id) deja que EF asigne la FK solo al
                    // guardar, ya que la EntregaProgramada todavía no tiene Id aquí.
                    EntregaProgramadaOrigen = entregaProgramada,
                    Detalles = productos.Select(p => new DetalleEntrega
                    {
                        Producto = p.Producto,
                        SistemaMedida = p.Unidad,
                        Total = p.Total
                    }).ToList()
                });
            }

            contexto.EntregasProgramadas.AddRange(entregasProgramadas);
            contexto.Entregas.AddRange(entregas);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} donaciones entregadas en el historial.", entregas.Count);
        }

        private const int MetaEntregasProgramadasSembradas = 50;

        private static async Task SembrarEntregasProgramadasAsync(ContextoViveres contexto, ILogger registrador)
        {
            // Solo se cuentan las que siguen "Programada": las completadas del historial
            // de donaciones (ver SembrarEntregasAsync) son otra cosa y no deben contar
            // para la meta de entregas por realizar.
            var existentes = await contexto.EntregasProgramadas
                .CountAsync(e => e.Estado == EstadoEntregaProgramada.Programada);
            var faltantes = MetaEntregasProgramadasSembradas - existentes;

            if (faltantes <= 0)
            {
                return;
            }

            // Se recorren los municipios de todos los departamentos para que los destinos
            // queden repartidos entre distintos municipios y departamentos de Guatemala.
            var municipiosConDepartamento = Catalogos.MunicipiosPorDepartamento
                .SelectMany(par => par.Value.Select(municipio => (Departamento: par.Key, Municipio: municipio)))
                .ToArray();

            var entregasProgramadas = new List<EntregaProgramada>();

            for (var i = 0; i < faltantes; i++)
            {
                var (departamento, municipio) = municipiosConDepartamento[
                    (int)((long)i * municipiosConDepartamento.Length / faltantes) % municipiosConDepartamento.Length];

                var productos = GenerarProductosSupuestos();

                entregasProgramadas.Add(new EntregaProgramada
                {
                    Lugar = $"Comunidad {municipio}",
                    Departamento = departamento,
                    Municipio = municipio,
                    FechaProgramada = DateOnly.FromDateTime(DateTime.Now.AddDays(Aleatorio.Next(1, 60))),
                    FamiliasBeneficiadas = Aleatorio.Next(15, 160),
                    Estado = EstadoEntregaProgramada.Programada,
                    RegistradoPor = "admin",
                    FechaRegistro = DateTime.UtcNow.AddDays(-Aleatorio.Next(1, 30)),
                    Detalles = productos.Select(p => new DetalleEntregaProgramada
                    {
                        Producto = p.Producto,
                        SistemaMedida = p.Unidad,
                        Total = p.Total
                    }).ToList()
                });
            }

            contexto.EntregasProgramadas.AddRange(entregasProgramadas);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} entregas por realizar.", entregasProgramadas.Count);
        }

        private const int MetaVisitasPreviasSembradas = 10;

        private static async Task SembrarVisitasPreviasAsync(ContextoViveres contexto, ILogger registrador)
        {
            var existentes = await contexto.VisitasPrevias.CountAsync();
            var faltantes = MetaVisitasPreviasSembradas - existentes;

            if (faltantes <= 0)
            {
                return;
            }

            var municipiosConDepartamento = Catalogos.MunicipiosPorDepartamento
                .SelectMany(par => par.Value.Select(municipio => (Departamento: par.Key, Municipio: municipio)))
                .ToArray();

            var visitas = new List<VisitaPrevia>();

            for (var i = 0; i < faltantes; i++)
            {
                // Se toman desde el final de la lista para no repetir los mismos
                // municipios que ya usaron las entregas programadas de ejemplo.
                var indice = municipiosConDepartamento.Length - 1 -
                    ((int)((long)i * municipiosConDepartamento.Length / faltantes) % municipiosConDepartamento.Length);
                var (departamento, municipio) = municipiosConDepartamento[indice];

                var productos = GenerarProductosSupuestos();

                visitas.Add(new VisitaPrevia
                {
                    Lugar = $"Comunidad {municipio}",
                    Departamento = departamento,
                    Municipio = municipio,
                    FechaPropuesta = DateOnly.FromDateTime(DateTime.Now.AddDays(Aleatorio.Next(3, 60))),
                    FamiliasBeneficiadas = Aleatorio.Next(15, 160),
                    Estado = EstadoVisitaPrevia.Pendiente,
                    RegistradoPor = "admin",
                    FechaRegistro = DateTime.UtcNow.AddDays(-Aleatorio.Next(1, 20)),
                    Detalles = productos.Select(p => new DetalleVisitaPrevia
                    {
                        Producto = p.Producto,
                        SistemaMedida = p.Unidad,
                        Total = p.Total
                    }).ToList()
                });
            }

            contexto.VisitasPrevias.AddRange(visitas);
            await contexto.SaveChangesAsync();

            registrador.LogInformation("Se sembraron {Cantidad} visitas previas.", visitas.Count);
        }
    }
}
