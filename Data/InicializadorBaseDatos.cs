using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // 4. Sembrar los datos de demostración (inventario, pedidos e historial).
            await SembrarAlimentosAsync(contexto, registrador);
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
            ("Pañales", "Otros", "Bolsas", true)
        ];

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

                // Cantidades entre 500 y 1900: con los 50 productos suman aprox. la mitad
                // de la meta de la campaña, para que la barra de progreso se vea llena.
                var cantidad = Aleatorio.Next(500, 1900);

                DateOnly? vencimiento = null;
                if (!producto.NoPerecedero)
                {
                    // Entre -15 (ya vencido) y +180 días (buen estado), con variedad
                    // suficiente para que existan artículos vencidos, por vencer y en buen estado.
                    var diasOffset = Aleatorio.Next(-15, 181);
                    vencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(diasOffset));
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

        private static readonly string[] Proveedores =
        [
            "Distribuidora El Sol", "Agroinsumos GT", "Mercado La Terminal", "Cooperativa Rural",
            "Almacenes La Económica", "Central de Abastos", "Distribuidora San José"
        ];

        private const int MetaPedidosSembrados = 40;

        private static async Task SembrarPedidosAsync(ContextoViveres contexto, ILogger registrador)
        {
            var existentes = await contexto.Pedidos.CountAsync();
            var faltantes = MetaPedidosSembrados - existentes;

            if (faltantes <= 0)
            {
                return;
            }

            var prioridades = new[] { PrioridadPedido.Baja, PrioridadPedido.Normal, PrioridadPedido.Alta };
            var estados = new[] { EstadoPedido.Pendiente, EstadoPedido.Solicitado };
            var pedidos = new List<Pedido>();

            for (var i = 0; i < faltantes; i++)
            {
                var producto = Productos[i % Productos.Length];

                pedidos.Add(new Pedido
                {
                    Articulo = producto.Nombre,
                    Categoria = producto.Categoria,
                    Cantidad = Aleatorio.Next(50, 500),
                    UnidadMedida = producto.Unidad,
                    Proveedor = Proveedores[Aleatorio.Next(Proveedores.Length)],
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

        /// <summary>
        /// Siembra el historial de "Donaciones entregadas". Como son entregas que ya
        /// pasaron y los víveres ya no están en bodega, cada una lleva su propia Entrega
        /// Programada "Completada" con productos supuestos (no hace falta que existan
        /// en el inventario actual): eso le da a cada fila su detalle de productos y su
        /// comprobante en PDF, igual que a las entregas completadas desde la aplicación.
        /// </summary>
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

        private static async Task SembrarEntregasAsync(ContextoViveres contexto, ILogger registrador)
        {
            // Backfill: registros históricos sembrados antes de que "Donaciones entregadas"
            // guardara detalle de productos y comprobante en PDF. Se completan con productos
            // supuestos (ya no están en bodega, así que no hace falta que existan en el
            // inventario actual) para que todas las filas terminen con su PDF.
            var sinDetalle = await contexto.Entregas
                .Where(e => e.EntregaProgramadaId == null)
                .ToListAsync();

            if (sinDetalle.Count > 0)
            {
                var programadasBackfill = new List<EntregaProgramada>();

                foreach (var entrega in sinDetalle)
                {
                    var productos = GenerarProductosSupuestos();

                    var entregaProgramada = new EntregaProgramada
                    {
                        Lugar = entrega.Lugar,
                        Departamento = entrega.Departamento,
                        Municipio = entrega.Municipio ?? string.Empty,
                        FechaProgramada = entrega.FechaEntrega,
                        FamiliasBeneficiadas = entrega.FamiliasBeneficiadas,
                        Estado = EstadoEntregaProgramada.Completada,
                        RegistradoPor = entrega.RegistradoPor,
                        FechaRegistro = entrega.FechaRegistro,
                        FechaCompletada = AFechaUtc(entrega.FechaEntrega),
                        Detalles = productos.Select(p => new DetalleEntregaProgramada
                        {
                            Producto = p.Producto,
                            SistemaMedida = p.Unidad,
                            Total = p.Total
                        }).ToList()
                    };
                    programadasBackfill.Add(entregaProgramada);

                    entrega.EntregaProgramadaOrigen = entregaProgramada;
                    entrega.TotalEntregado = productos.Sum(p => p.Total);
                    entrega.Detalles = productos.Select(p => new DetalleEntrega
                    {
                        Producto = p.Producto,
                        SistemaMedida = p.Unidad,
                        Total = p.Total
                    }).ToList();
                }

                contexto.EntregasProgramadas.AddRange(programadasBackfill);
                await contexto.SaveChangesAsync();

                registrador.LogInformation(
                    "Se completó el detalle y comprobante de {Cantidad} donaciones entregadas antiguas.", sinDetalle.Count);
            }

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
