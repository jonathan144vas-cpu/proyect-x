using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControlViveresApp.Controllers
{
    public class CuentasController : Controller
    {
        private readonly SignInManager<Usuario> _gestorSesion;
        private readonly UserManager<Usuario> _gestorUsuarios;

        public CuentasController(SignInManager<Usuario> gestorSesion, UserManager<Usuario> gestorUsuarios)
        {
            _gestorSesion = gestorSesion;
            _gestorUsuarios = gestorUsuarios;
        }

        [HttpGet]
        public IActionResult IniciarSesion(string? urlRetorno = null)
        {
            // Si ya inició sesión, no tiene sentido mostrarle el formulario otra vez.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Inicio");
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

                return RedirectToAction("Index", "Inicio");
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

        [HttpGet]
        public IActionResult Registro(string? urlRetorno = null)
        {
            // Si ya inició sesión, no tiene sentido mostrarle el formulario otra vez.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Inicio");
            }

            ViewData["UrlRetorno"] = urlRetorno;
            return View(new Registro());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Registro modelo, string? urlRetorno = null)
        {
            ViewData["UrlRetorno"] = urlRetorno;

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario
            {
                UserName = modelo.NombreUsuario,
                Email = modelo.Correo,
                NombreCompleto = modelo.NombreCompleto,
                EmailConfirmed = true
            };

            // Las cuentas públicas nacen con el rol "Usuario" (solo lectura); un
            // administrador puede ascenderlas más adelante si confía en esa persona.
            var resultado = await _gestorUsuarios.CreateAsync(usuario, modelo.Contrasena);

            if (resultado.Succeeded)
            {
                await _gestorUsuarios.AddToRoleAsync(usuario, "Usuario");
                await _gestorSesion.SignInAsync(usuario, isPersistent: false);

                if (!string.IsNullOrEmpty(urlRetorno) && Url.IsLocalUrl(urlRetorno))
                {
                    return Redirect(urlRetorno);
                }

                return RedirectToAction("Index", "Inicio");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
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
