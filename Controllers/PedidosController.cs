using ControlViveresApp.Data;
using ControlViveresApp.Models;
using ControlViveresApp.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ContextoViveres _contexto;
        private readonly ServicioComprobantesPdf _servicioPdf;

        public PedidosController(ContextoViveres contexto, ServicioComprobantesPdf servicioPdf)
        {
            _contexto = contexto;
            _servicioPdf = servicioPdf;
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
                // Primero lo que sigue pendiente, y dentro de eso lo que se necesita más
                // pronto (los sin fecha definida quedan al final de su grupo).
                .OrderBy(p => p.Estado == EstadoPedido.Recibido || p.Estado == EstadoPedido.Cancelado)
                .ThenBy(p => p.FechaNecesaria == null ? 1 : 0)
                .ThenBy(p => p.FechaNecesaria)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            return View(listaPedidos);
        }

        // 2. CREAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(string? articulo, string? categoria, string? unidadMedida)
        {
            await CargarProveedoresAsync();

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
            [Bind("Articulo,Categoria,Cantidad,UnidadMedida,ProveedorId,FechaNecesaria,Prioridad,Estado,Observaciones")] Pedido pedido)
        {
            ValidarCatalogos(pedido);
            await ValidarProveedorAsync(pedido);

            if (!ModelState.IsValid)
            {
                await CargarProveedoresAsync();
                return View(pedido);
            }

            pedido.FechaSolicitud = DateTime.UtcNow;
            pedido.SolicitadoPor = User.Identity?.Name;

            _contexto.Pedidos.Add(pedido);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido registrado correctamente";
            TempData["Tipo"] = "success";
            TempData["FilaNuevaId"] = pedido.Id;

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var pedido = await _contexto.Pedidos.FindAsync(id);
            if (pedido is null) return NotFound();

            if (pedido.Estado == EstadoPedido.Recibido)
            {
                TempData["Mensaje"] = "Ese pedido ya fue recibido: no se puede editar, solo consultar su comprobante";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index));
            }

            await CargarProveedoresAsync();
            return View(pedido);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Articulo,Categoria,Cantidad,UnidadMedida,ProveedorId,FechaNecesaria,Prioridad,Estado,Observaciones")] Pedido pedido)
        {
            if (id != pedido.Id) return NotFound();

            ValidarCatalogos(pedido);
            await ValidarProveedorAsync(pedido);

            if (!ModelState.IsValid)
            {
                await CargarProveedoresAsync();
                return View(pedido);
            }

            var existente = await _contexto.Pedidos.FindAsync(id);
            if (existente is null) return NotFound();

            if (existente.Estado == EstadoPedido.Recibido)
            {
                TempData["Mensaje"] = "Ese pedido ya fue recibido: no se puede editar, solo consultar su comprobante";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index));
            }

            // FechaSolicitud y SolicitadoPor no se tocan: son el historial del pedido.
            existente.Articulo = pedido.Articulo;
            existente.Categoria = pedido.Categoria;
            existente.Cantidad = pedido.Cantidad;
            existente.UnidadMedida = pedido.UnidadMedida;

            // Un pedido viejo puede tener solo el nombre del proveedor escrito a mano. Ese texto
            // se respeta mientras no se elija uno de la lista (o se quite uno ya elegido).
            if (pedido.ProveedorId is not null || existente.ProveedorId is not null)
            {
                existente.Proveedor = pedido.Proveedor;
            }

            existente.ProveedorId = pedido.ProveedorId;
            existente.FechaNecesaria = pedido.FechaNecesaria;
            existente.Prioridad = pedido.Prioridad;
            existente.Estado = pedido.Estado;
            existente.Observaciones = pedido.Observaciones;

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido actualizado correctamente";
            TempData["Tipo"] = "success";
            TempData["FilaNuevaId"] = existente.Id;

            return RedirectToAction(nameof(Index));
        }

        // 6. MARCAR COMO RECIBIDO (y pasar la cantidad al inventario)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Recibir(int id, DateOnly? fechaVencimiento)
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

            // Si el pedido no traía fecha de vencimiento, se usa la que se escribió al recibirlo,
            // para que el producto no entre al inventario "sin fecha".
            if (pedido.FechaVencimiento is null && fechaVencimiento is not null)
            {
                pedido.FechaVencimiento = fechaVencimiento;
            }

            // Si ya existe ese artículo en el inventario (mismo nombre, misma unidad y
            // misma fecha de vencimiento), se le suma la cantidad. Si no existe, o si
            // existe pero con una fecha de vencimiento distinta, se registra como un
            // lote/registro separado.
            var nombreNormalizado = pedido.Articulo.Trim().ToLower();

            var alimento = await _contexto.Alimentos.FirstOrDefaultAsync(a =>
                a.Nombre.ToLower() == nombreNormalizado &&
                a.UnidadMedida == pedido.UnidadMedida &&
                a.FechaVencimiento == pedido.FechaVencimiento);

            if (alimento is null)
            {
                _contexto.Alimentos.Add(new Alimento
                {
                    Nombre = pedido.Articulo.Trim(),
                    Categoria = pedido.Categoria,
                    Cantidad = pedido.Cantidad,
                    UnidadMedida = pedido.UnidadMedida,
                    FechaVencimiento = pedido.FechaVencimiento,
                    FechaRegistro = DateTime.UtcNow
                });
            }
            else
            {
                alimento.Cantidad += pedido.Cantidad;
            }

            pedido.Estado = EstadoPedido.Recibido;

            _contexto.MovimientosInventario.Add(MovimientoInventario.Nuevo(
                TipoMovimiento.Entrada, pedido.Articulo.Trim(), pedido.Categoria, pedido.Cantidad,
                pedido.UnidadMedida, $"Pedido #{pedido.Id}", User.Identity?.Name));

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Pedido recibido. Se agregaron {pedido.Cantidad} {pedido.UnidadMedida} de {pedido.Articulo} al inventario.";
            TempData["Tipo"] = "success";
            TempData["FilaNuevaId"] = pedido.Id;
            TempData["ComprobanteRecepcionId"] = pedido.Id;

            return RedirectToAction(nameof(Index));
        }

        // 7. COMPROBANTE PDF DE RECEPCIÓN
        public async Task<IActionResult> ComprobanteRecepcion(int id)
        {
            var pedido = await _contexto.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (pedido is null || pedido.Estado != EstadoPedido.Recibido)
            {
                return NotFound();
            }

            var pdf = _servicioPdf.GenerarComprobanteRecepcion(pedido);
            return File(pdf, "application/pdf", $"Recepcion-Pedido-{pedido.Id}.pdf");
        }

        // 8. ELIMINAR
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

        private async Task CargarProveedoresAsync()
        {
            ViewBag.Proveedores = await _contexto.Proveedores.AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorOpcion(p.Id, p.Nombre, p.Categoria))
                .ToListAsync();
        }

        /// <summary>
        /// Comprueba que el proveedor exista y sea del área del pedido, y copia su nombre
        /// al pedido. Sin esto, alguien podría enviar el formulario con un proveedor de otra área.
        /// </summary>
        private async Task ValidarProveedorAsync(Pedido pedido)
        {
            pedido.Proveedor = null;

            if (pedido.ProveedorId is null)
            {
                return;
            }

            var proveedor = await _contexto.Proveedores.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pedido.ProveedorId);

            if (proveedor is null || proveedor.Categoria != pedido.Categoria)
            {
                ModelState.AddModelError(nameof(Pedido.ProveedorId), "Elige un proveedor de la categoría seleccionada");
                return;
            }

            pedido.Proveedor = proveedor.Nombre;
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
