using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var constructor = WebApplication.CreateBuilder(args);

// 1. Conexión a PostgreSQL
var cadenaConexion = constructor.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'PostgresConnection' en appsettings.json.");

constructor.Services.AddDbContext<ContextoViveres>(opciones =>
    opciones.UseNpgsql(cadenaConexion));

// 2. Configurar Identity (usuarios, contraseñas y roles)
constructor.Services.AddIdentity<Usuario, IdentityRole>(opciones =>
{
    // Reglas sencillas para contraseñas de prueba.
    // Para un despliegue real conviene subir RequiredLength y exigir dígito/mayúscula.
    opciones.Password.RequireDigit = false;
    opciones.Password.RequiredLength = 4;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireLowercase = false;

    // Bloqueo tras varios intentos fallidos, contra ataques de fuerza bruta.
    opciones.Lockout.MaxFailedAccessAttempts = 5;
    opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

    opciones.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ContextoViveres>()
.AddDefaultTokenProviders();

// 3. Rutas de redirección de la cookie de autenticación
constructor.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Cuentas/IniciarSesion";
    opciones.LogoutPath = "/Cuentas/CerrarSesion";
    opciones.AccessDeniedPath = "/Cuentas/AccesoDenegado";
    opciones.ReturnUrlParameter = "urlRetorno";
    opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
    opciones.SlidingExpiration = true;
});

// Datos de la campaña (nombre, destino y meta), leídos de appsettings.json
constructor.Services.Configure<OpcionesCampania>(
    constructor.Configuration.GetSection(OpcionesCampania.Seccion));

constructor.Services.AddControllersWithViews();

var app = constructor.Build();

// 4. Aplicar migraciones y sembrar roles/usuarios iniciales
using (var ambito = app.Services.CreateScope())
{
    try
    {
        await InicializadorBaseDatos.InicializarAsync(ambito.ServiceProvider);
    }
    catch (Exception ex)
    {
        // Se registra y se relanza: si la base de datos no está lista, es mejor que la
        // aplicación no arranque a que arranque rota y falle en la primera pantalla.
        app.Logger.LogError(ex, "Error al preparar la base de datos.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Inicio/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANTE: Authentication debe ir antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Alimentos}/{action=Index}/{id?}");

app.Run();
