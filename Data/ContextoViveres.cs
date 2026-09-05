using ControlViveresApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core contra PostgreSQL.
    /// Incluye las tablas de Identity (usuarios y roles), el inventario de víveres
    /// y los pedidos por realizar.
    /// </summary>
    public class ContextoViveres : IdentityDbContext<Usuario>
    {
        public ContextoViveres(DbContextOptions<ContextoViveres> opciones) : base(opciones)
        {
        }

        public DbSet<Alimento> Alimentos => Set<Alimento>();

        public DbSet<Pedido> Pedidos => Set<Pedido>();

        public DbSet<Entrega> Entregas => Set<Entrega>();

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
            });

            constructor.Entity<Entrega>(entrega =>
            {
                entrega.HasIndex(e => e.Departamento);
                entrega.HasIndex(e => e.FechaEntrega);
            });
        }
    }
}
