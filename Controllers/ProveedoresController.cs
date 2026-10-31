using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    /// <summary>
    /// Alta de proveedores. No tiene pantallas propias: se usa desde el formulario de
    /// pedidos, en la opción "Agregar proveedor" de la lista, y responde en JSON.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ProveedoresController : Controller
    {
        private readonly ContextoViveres _contexto;

        public ProveedoresController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [Bind("Nombre,Categoria,Telefono,Direccion,AsesorEncargado,Correo,Observaciones")] Proveedor proveedor)
        {
            proveedor.Nombre = proveedor.Nombre?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(proveedor.Categoria) &&
                !Catalogos.Categorias.Contains(proveedor.Categoria))
            {
                ModelState.AddModelError(nameof(Proveedor.Categoria), "El área seleccionada no es válida");
            }

            var yaExiste = await _contexto.Proveedores.AnyAsync(p =>
                p.Categoria == proveedor.Categoria && p.Nombre.ToLower() == proveedor.Nombre.ToLower());

            if (yaExiste)
            {
                ModelState.AddModelError(nameof(Proveedor.Nombre), "Ya hay un proveedor con ese nombre en esta área");
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

                return BadRequest(new { errores });
            }

            proveedor.Alta = new NuevoProveedor
            {
                RegistradoPor = User.Identity?.Name,
                FechaRegistro = DateTime.UtcNow,
                Origen = "Formulario de pedidos"
            };

            _contexto.Proveedores.Add(proveedor);
            await _contexto.SaveChangesAsync();

            return Json(new ProveedorOpcion(proveedor.Id, proveedor.Nombre, proveedor.Categoria));
        }
    }
}
