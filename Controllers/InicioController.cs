using System.Diagnostics;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControlViveresApp.Controllers;

public class InicioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacidad()
    {
        return View();
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
