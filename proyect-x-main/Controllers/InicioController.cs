using System.Diagnostics;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControlViveresApp.Controllers;

public class InicioController : Controller
{
    public IActionResult Index()
    {
        // Sin sesión: pantalla de bienvenida pública, sin datos reales ni menú.
        // Con sesión: panel con accesos rápidos a cada sección del sistema.
        return View(User.Identity?.IsAuthenticated == true ? "Panel" : "Bienvenida");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ModeloError
        {
            IdSolicitud = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
