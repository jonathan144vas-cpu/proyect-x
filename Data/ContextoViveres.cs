using ControlViveresApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core contra PostgreSQL.
    /// Incluye las tablas de Identity (usuarios y roles), el inventario de víveres,
    /// los pedidos por realizar y los proveedores.
    /// </summary>
    public class ContextoViveres : IdentityDbContext<Usuario>
    {
        public ContextoViveres(DbContextOptions<ContextoViveres> opciones) : base(opciones)
        {
        }

        public DbSet<Alimento> Alimentos => Set<Alimento>();

        public DbSet<Pedido> Pedidos => Set<Pedido>();

        public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

        public DbSet<Proveedor> Proveedores => Set<Proveedor>();

        public DbSet<NuevoProveedor> NuevosProveedores => Set<NuevoProveedor>();

        public DbSet<Entrega> Entregas => Set<Entrega>();

        public DbSet<EntregaProgramada> EntregasProgramadas => Set<EntregaProgramada>();

        public DbSet<DetalleEntregaProgramada> DetallesEntregaProgramada => Set<DetalleEntregaProgramada>();

        public DbSet<DetalleEntrega> DetallesEntrega => Set<DetalleEntrega>();

        public DbSet<VisitaPrevia> VisitasPrevias => Set<VisitaPrevia>();

        public DbSet<DetalleVisitaPrevia> DetallesVisitaPrevia => Set<DetalleVisitaPrevia>();

        protected override void OnModelCreating(ModelBuilder constructor)
        {
            base.OnModelCreating(constructor);

            // Nombres de tabla en español. Identity sigue funcionando igual:
            // internamente usa los nombres de las propiedades, no los de las tablas.
            constructor.Entity<Usuario>().ToTable("Usuarios");
            constructor.Entity<IdentityRole>().ToTable("Roles");
            constructor.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
            constructor.Entity<IdentityUserClaim<string>>().ToTable("UsuariosClaims");
            constructor.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
            constructor.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");
            constructor.Entity<IdentityRoleClaim<string>>().ToTable("RolesClaims");

            // Índices para los filtros de la pantalla de inventario.
            constructor.Entity<Alimento>(alimento =>
            {
                alimento.HasIndex(a => a.Categoria);
                alimento.HasIndex(a => a.FechaVencimiento);
            });

            constructor.Entity<Pedido>(pedido =>
            {
                // Los enums se guardan como texto ("Pendiente", "Alta"...) en vez de números,
                // para que la tabla se pueda leer directamente desde pgAdmin.
                pedido.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
                pedido.Property(p => p.Prioridad).HasConversion<string>().HasMaxLength(20);

                pedido.HasIndex(p => p.Estado);
                pedido.HasIndex(p => p.Categoria);

                // Si se borra un proveedor, sus pedidos se quedan con el nombre guardado.
                pedido.HasOne(p => p.ProveedorAsignado)
                    .WithMany()
                    .HasForeignKey(p => p.ProveedorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            constructor.Entity<MovimientoInventario>(movimiento =>
            {
                movimiento.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
                movimiento.HasIndex(m => m.Fecha);
            });

            constructor.Entity<Proveedor>(proveedor =>
            {
                proveedor.HasIndex(p => p.Categoria);
                proveedor.HasIndex(p => new { p.Nombre, p.Categoria }).IsUnique();

                proveedor.HasOne(p => p.Alta)
                    .WithOne(a => a.Proveedor)
                    .HasForeignKey<NuevoProveedor>(a => a.ProveedorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            constructor.Entity<Entrega>(entrega =>
            {
                entrega.HasIndex(e => e.Departamento);
                entrega.HasIndex(e => e.FechaEntrega);

                entrega.HasMany(e => e.Detalles)
                    .WithOne(d => d.Entrega)
                    .HasForeignKey(d => d.EntregaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entrega.HasOne(e => e.EntregaProgramadaOrigen)
                    .WithMany()
                    .HasForeignKey(e => e.EntregaProgramadaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            constructor.Entity<EntregaProgramada>(entregaProgramada =>
            {
                entregaProgramada.Property(e => e.Estado).HasConversion<string>().HasMaxLength(20);

                entregaProgramada.HasIndex(e => e.Departamento);
                entregaProgramada.HasIndex(e => e.Estado);

                entregaProgramada.HasMany(e => e.Detalles)
                    .WithOne(d => d.EntregaProgramada)
                    .HasForeignKey(d => d.EntregaProgramadaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            constructor.Entity<VisitaPrevia>(visita =>
            {
                visita.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20);

                visita.HasIndex(v => v.Departamento);
                visita.HasIndex(v => v.Estado);

                visita.HasMany(v => v.Detalles)
                    .WithOne(d => d.VisitaPrevia)
                    .HasForeignKey(d => d.VisitaPreviaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
