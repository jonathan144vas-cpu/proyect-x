using System.Threading.Tasks;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlViveresApp.Controllers
{
    public class CuentasController : Controller
    {
        private readonly SignInManager<Usuario> _gestorSesion;

        public CuentasController(SignInManager<Usuario> gestorSesion)
        {
            _gestorSesion = gestorSesion;
        }

        [HttpGet]
        public IActionResult IniciarSesion(string? urlRetorno = null)
        {
            // Si ya inició sesión, no tiene sentido mostrarle el formulario otra vez.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Alimentos");
            }

            ViewData["UrlRetorno"] = urlRetorno;
            return View(new InicioSesion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(InicioSesion modelo, string? urlRetorno = null)
        {
            ViewData["UrlRetorno"] = urlRetorno;

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = await _gestorSesion.PasswordSignInAsync(
                modelo.NombreUsuario,
                modelo.Contrasena,
                isPersistent: modelo.Recordarme,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                // Url.IsLocalUrl evita que alguien redirija a un sitio externo con ?urlRetorno=
                if (!string.IsNullOrEmpty(urlRetorno) && Url.IsLocalUrl(urlRetorno))
                {
                    return Redirect(urlRetorno);
                }

                return RedirectToAction("Index", "Alimentos");
            }

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La cuenta está bloqueada temporalmente por demasiados intentos fallidos. Vuelve a intentarlo en unos minutos.");
            }
            else
            {
                // Mensaje genérico a propósito: no revela si el usuario existe o no.
                ModelState.AddModelError(string.Empty, "El usuario o la contraseña no son correctos.");
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CerrarSesion()
        {
            await _gestorSesion.SignOutAsync();
            return RedirectToAction(nameof(IniciarSesion));
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
