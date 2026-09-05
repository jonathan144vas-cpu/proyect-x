using System;
using System.Linq;
using System.Threading.Tasks;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Data
{
    /// <summary>
    /// Aplica las migraciones pendientes y siembra los roles y usuarios iniciales.
    /// Se ejecuta una sola vez, al arrancar la aplicación.
    /// </summary>
    public static class InicializadorBaseDatos
    {
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
    }
}
