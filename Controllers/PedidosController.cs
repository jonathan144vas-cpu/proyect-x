using System;
using System.Linq;
using System.Threading.Tasks;
using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ContextoViveres _contexto;

        public PedidosController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        // 1. LISTAR PEDIDOS
        public async Task<IActionResult> Index(string? buscar, EstadoPedido? estado)
        {
            var consulta = _contexto.Pedidos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                consulta = consulta.Where(p => p.Articulo.ToLower().Contains(texto));
            }

            if (estado is not null)
            {
                consulta = consulta.Where(p => p.Estado == estado);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var listaPedidos = await consulta
                // Primero lo que sigue pendiente, y dentro de eso lo más urgente.
                .OrderBy(p => p.Estado == EstadoPedido.Recibido || p.Estado == EstadoPedido.Cancelado)
                .ThenByDescending(p => p.Prioridad)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            return View(listaPedidos);
        }

        // 2. CREAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public IActionResult Crear(string? articulo, string? categoria, string? unidadMedida)
        {
            // Los parámetros permiten llegar aquí desde el inventario con el formulario prellenado.
            return View(new Pedido
            {
                Articulo = articulo ?? string.Empty,
                Categoria = categoria ?? string.Empty,
                UnidadMedida = unidadMedida ?? string.Empty
            });
        }

        // 3. GUARDAR NUEVO PEDIDO
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(
            [Bind("Articulo,Categoria,Cantidad,UnidadMedida,Proveedor,FechaNecesaria,Prioridad,Estado,Observaciones")] Pedido pedido)
        {
            ValidarCatalogos(pedido);

            if (!ModelState.IsValid)
            {
                return View(pedido);
            }

            pedido.FechaSolicitud = DateTime.UtcNow;
            pedido.SolicitadoPor = User.Identity?.Name;

            _contexto.Pedidos.Add(pedido);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido registrado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var pedido = await _contexto.Pedidos.FindAsync(id);
            if (pedido is null) return NotFound();

            return View(pedido);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Articulo,Categoria,Cantidad,UnidadMedida,Proveedor,FechaNecesaria,Prioridad,Estado,Observaciones")] Pedido pedido)
        {
            if (id != pedido.Id) return NotFound();

            ValidarCatalogos(pedido);

            if (!ModelState.IsValid)
            {
                return View(pedido);
            }

            var existente = await _contexto.Pedidos.FindAsync(id);
            if (existente is null) return NotFound();

            // FechaSolicitud y SolicitadoPor no se tocan: son el historial del pedido.
            existente.Articulo = pedido.Articulo;
            existente.Categoria = pedido.Categoria;
            existente.Cantidad = pedido.Cantidad;
            existente.UnidadMedida = pedido.UnidadMedida;
            existente.Proveedor = pedido.Proveedor;
            existente.FechaNecesaria = pedido.FechaNecesaria;
            existente.Prioridad = pedido.Prioridad;
            existente.Estado = pedido.Estado;
            existente.Observaciones = pedido.Observaciones;

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido actualizado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 6. MARCAR COMO RECIBIDO (y pasar la cantidad al inventario)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Recibir(int id)
        {
            var pedido = await _contexto.Pedidos.FindAsync(id);

            if (pedido is null)
            {
                TempData["Mensaje"] = "El pedido ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado == EstadoPedido.Recibido)
            {
                TempData["Mensaje"] = "Ese pedido ya estaba marcado como recibido";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index));
            }

            // Si ya existe ese artículo en el inventario (mismo nombre y misma unidad),
            // se le suma la cantidad. Si no existe, se crea el renglón.
            var nombreNormalizado = pedido.Articulo.Trim().ToLower();

            var alimento = await _contexto.Alimentos.FirstOrDefaultAsync(a =>
                a.Nombre.ToLower() == nombreNormalizado &&
                a.UnidadMedida == pedido.UnidadMedida);

            if (alimento is null)
            {
                _contexto.Alimentos.Add(new Alimento
                {
                    Nombre = pedido.Articulo.Trim(),
                    Categoria = pedido.Categoria,
                    Cantidad = pedido.Cantidad,
                    UnidadMedida = pedido.UnidadMedida,
                    FechaRegistro = DateTime.UtcNow
                });
            }
            else
            {
                alimento.Cantidad += pedido.Cantidad;
            }

            pedido.Estado = EstadoPedido.Recibido;

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Pedido recibido. Se agregaron {pedido.Cantidad} {pedido.UnidadMedida} de {pedido.Articulo} al inventario.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 7. ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var pedido = await _contexto.Pedidos.FindAsync(id);

            if (pedido is null)
            {
                TempData["Mensaje"] = "El pedido ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _contexto.Pedidos.Remove(pedido);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido eliminado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private void ValidarCatalogos(Pedido pedido)
        {
            if (!string.IsNullOrWhiteSpace(pedido.Categoria) &&
                !Catalogos.Categorias.Contains(pedido.Categoria))
            {
                ModelState.AddModelError(nameof(Pedido.Categoria), "La categoría seleccionada no es válida");
            }

            if (!string.IsNullOrWhiteSpace(pedido.UnidadMedida) &&
                !Catalogos.UnidadesMedida.Contains(pedido.UnidadMedida))
            {
                ModelState.AddModelError(nameof(Pedido.UnidadMedida), "La unidad de medida seleccionada no es válida");
            }
        }
    }
}
