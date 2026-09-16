using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlViveresApp.Data;
using ControlViveresApp.Models;
using ControlViveresApp.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    /// <summary>
    /// Entregas programadas hacia lugares por visitar. Al completarlas, se descuenta
    /// el inventario, se registra el lugar en el historial de "Lugares Visitados"
    /// y se genera un comprobante en PDF.
    /// </summary>
    [Authorize]
    public class EntregasProgramadasController : Controller
    {
        private readonly ContextoViveres _contexto;
        private readonly ServicioComprobantesPdf _servicioPdf;

        public EntregasProgramadasController(ContextoViveres contexto, ServicioComprobantesPdf servicioPdf)
        {
            _contexto = contexto;
            _servicioPdf = servicioPdf;
        }

        // 1. LISTAR
        public async Task<IActionResult> Index(string? buscar, string? departamento, EstadoEntregaProgramada? estado)
        {
            var consulta = _contexto.EntregasProgramadas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                consulta = consulta.Where(e => e.Lugar.ToLower().Contains(texto)
                                            || e.Municipio.ToLower().Contains(texto));
            }

            if (!string.IsNullOrWhiteSpace(departamento))
            {
                consulta = consulta.Where(e => e.Departamento == departamento);
            }

            if (estado is not null)
            {
                consulta = consulta.Where(e => e.Estado == estado);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Departamento = departamento;
            ViewBag.Estado = estado;

            ViewBag.TotalProgramadas = await _contexto.EntregasProgramadas
                .CountAsync(e => e.Estado == EstadoEntregaProgramada.Programada);
            ViewBag.TotalCompletadas = await _contexto.EntregasProgramadas
                .CountAsync(e => e.Estado == EstadoEntregaProgramada.Completada);

            var lista = await consulta.ToListAsync();

            // Orden pensado para operar el día a día: primero lo programado (lo más
            // próximo a entregarse primero), luego lo ya completado (lo más reciente
            // primero, para ver de inmediato la última entrega real) y al final lo
            // cancelado. Se ordena en memoria porque mezcla FechaProgramada (DateOnly)
            // y FechaCompletada (DateTime), que no conviene combinar en una sola
            // consulta SQL.
            lista = lista
                .OrderBy(e => e.Estado switch
                {
                    EstadoEntregaProgramada.Programada => 0,
                    EstadoEntregaProgramada.Completada => 1,
                    _ => 2
                })
                .ThenBy(e => e.Estado == EstadoEntregaProgramada.Programada ? e.FechaProgramada : DateOnly.MaxValue)
                .ThenByDescending(e => e.FechaCompletada ?? DateTime.MinValue)
                .ThenByDescending(e => e.Id)
                .ToList();

            return View(lista);
        }

        // 2. CREAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public IActionResult Crear()
        {
            var entrega = new EntregaProgramada
            {
                FechaProgramada = DateOnly.FromDateTime(DateTime.Now)
            };
            entrega.Detalles.Add(new DetalleEntregaProgramada());

            return View(entrega);
        }

        // 3. GUARDAR NUEVO
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(
            [Bind("Lugar,Departamento,Municipio,FechaProgramada,FamiliasBeneficiadas,Observaciones,Detalles")] EntregaProgramada entrega)
        {
            ValidarUbicacion(entrega);
            LimpiarDetallesVacios(entrega);

            if (!ModelState.IsValid)
            {
                return View(entrega);
            }

            entrega.RegistradoPor = User.Identity?.Name;
            entrega.FechaRegistro = DateTime.UtcNow;
            entrega.Estado = EstadoEntregaProgramada.Programada;

            _contexto.EntregasProgramadas.Add(entrega);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Se programó la entrega a {entrega.Lugar}";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var entrega = await _contexto.EntregasProgramadas
                .Include(e => e.Detalles)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrega is null) return NotFound();
            if (entrega.Detalles.Count == 0) entrega.Detalles.Add(new DetalleEntregaProgramada());

            return View(entrega);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Lugar,Departamento,Municipio,FechaProgramada,FamiliasBeneficiadas,Observaciones,Detalles")] EntregaProgramada entrega)
        {
            if (id != entrega.Id) return NotFound();

            ValidarUbicacion(entrega);
            LimpiarDetallesVacios(entrega);

            if (!ModelState.IsValid)
            {
                return View(entrega);
            }

            var existente = await _contexto.EntregasProgramadas
                .Include(e => e.Detalles)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existente is null) return NotFound();

            if (existente.Estado != EstadoEntregaProgramada.Programada)
            {
                TempData["Mensaje"] = "Solo se pueden editar entregas que siguen programadas";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            existente.Lugar = entrega.Lugar;
            existente.Departamento = entrega.Departamento;
            existente.Municipio = entrega.Municipio;
            existente.FechaProgramada = entrega.FechaProgramada;
            existente.FamiliasBeneficiadas = entrega.FamiliasBeneficiadas;
            existente.Observaciones = entrega.Observaciones;

            _contexto.DetallesEntregaProgramada.RemoveRange(existente.Detalles);
            existente.Detalles = entrega.Detalles
                .Select(d => new DetalleEntregaProgramada
                {
                    Producto = d.Producto,
                    SistemaMedida = d.SistemaMedida,
                    Total = d.Total
                })
                .ToList();

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Entrega programada actualizada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 6. COMPLETAR (descuenta inventario, registra historial y habilita el PDF)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Completar(int id)
        {
            var entrega = await _contexto.EntregasProgramadas
                .Include(e => e.Detalles)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrega is null)
            {
                TempData["Mensaje"] = "Esa entrega ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (entrega.Estado != EstadoEntregaProgramada.Programada)
            {
                TempData["Mensaje"] = "Esa entrega ya no está programada";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index));
            }

            // Descuento FIFO: por cada producto solicitado se consume primero el lote
            // que vence más pronto, y se salta cualquier lote ya vencido para no
            // enviar producto caducado (esto cubre también el caso de una entrega que
            // se completa tarde y el lote que le tocaba ya expiró para entonces).
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var avisos = new List<string>();

            foreach (var detalle in entrega.Detalles)
            {
                var nombreNormalizado = detalle.Producto.Trim().ToLower();
                var restante = detalle.Total;

                var lotes = await _contexto.Alimentos
                    .Where(a => a.Nombre.ToLower() == nombreNormalizado && a.UnidadMedida == detalle.SistemaMedida)
                    .OrderBy(a => a.FechaVencimiento == null ? 1 : 0)
                    .ThenBy(a => a.FechaVencimiento)
                    .ToListAsync();

                var lotesVencidosSaltados = 0;

                foreach (var lote in lotes)
                {
                    if (restante <= 0) break;

                    if (lote.FechaVencimiento is DateOnly vencimiento && vencimiento < hoy)
                    {
                        lotesVencidosSaltados++;
                        continue;
                    }

                    var aDescontar = Math.Min(lote.Cantidad, (int)restante);
                    lote.Cantidad -= aDescontar;
                    restante -= aDescontar;
                }

                if (lotesVencidosSaltados > 0)
                {
                    avisos.Add($"{detalle.Producto}: se omitieron {lotesVencidosSaltados} lote(s) vencido(s).");
                }

                if (restante > 0)
                {
                    avisos.Add($"{detalle.Producto}: faltaron {restante} {detalle.SistemaMedida} (inventario insuficiente).");
                }
            }

            entrega.Estado = EstadoEntregaProgramada.Completada;
            entrega.FechaCompletada = DateTime.UtcNow;

            _contexto.Entregas.Add(new Entrega
            {
                Lugar = entrega.Lugar,
                Departamento = entrega.Departamento,
                Municipio = entrega.Municipio,
                FechaEntrega = DateOnly.FromDateTime(DateTime.Now),
                FamiliasBeneficiadas = entrega.FamiliasBeneficiadas,
                TotalEntregado = (int)entrega.Detalles.Sum(d => d.Total),
                Observaciones = entrega.Observaciones,
                RegistradoPor = User.Identity?.Name,
                FechaRegistro = DateTime.UtcNow,
                EntregaProgramadaId = entrega.Id,
                Detalles = entrega.Detalles.Select(d => new DetalleEntrega
                {
                    Producto = d.Producto,
                    SistemaMedida = d.SistemaMedida,
                    Total = d.Total
                }).ToList()
            });

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = avisos.Count == 0
                ? $"Entrega a {entrega.Lugar} completada. Se descontó del inventario y quedó en el historial."
                : $"Entrega a {entrega.Lugar} completada con observaciones: {string.Join(" ", avisos)}";
            TempData["Tipo"] = avisos.Count == 0 ? "success" : "warning";
            TempData["ComprobanteEntregaId"] = entrega.Id;

            return RedirectToAction(nameof(Index));
        }

        // 7. CANCELAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var entrega = await _contexto.EntregasProgramadas.FindAsync(id);

            if (entrega is null || entrega.Estado != EstadoEntregaProgramada.Programada)
            {
                TempData["Mensaje"] = "Esa entrega ya no se puede cancelar";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            entrega.Estado = EstadoEntregaProgramada.Cancelada;
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Entrega programada cancelada";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 8. ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var entrega = await _contexto.EntregasProgramadas.FindAsync(id);

            if (entrega is null)
            {
                TempData["Mensaje"] = "Esa entrega ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (entrega.Estado == EstadoEntregaProgramada.Completada)
            {
                TempData["Mensaje"] = "No se puede eliminar una entrega ya completada";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _contexto.EntregasProgramadas.Remove(entrega);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Entrega programada eliminada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 9. COMPROBANTE PDF DE ENTREGA
        public async Task<IActionResult> Comprobante(int id)
        {
            var entrega = await _contexto.EntregasProgramadas
                .AsNoTracking()
                .Include(e => e.Detalles)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrega is null || entrega.Estado != EstadoEntregaProgramada.Completada)
            {
                return NotFound();
            }

            var pdf = _servicioPdf.GenerarComprobanteEntrega(entrega);
            return File(pdf, "application/pdf", $"Entrega-{entrega.Id}.pdf");
        }

        private void ValidarUbicacion(EntregaProgramada entrega)
        {
            if (!string.IsNullOrWhiteSpace(entrega.Departamento) &&
                Catalogos.BuscarDepartamento(entrega.Departamento) is null)
            {
                ModelState.AddModelError(nameof(EntregaProgramada.Departamento),
                    "El departamento seleccionado no es de Guatemala");
                return;
            }

            if (!string.IsNullOrWhiteSpace(entrega.Municipio) &&
                !Catalogos.MunicipiosDe(entrega.Departamento).Contains(entrega.Municipio))
            {
                ModelState.AddModelError(nameof(EntregaProgramada.Municipio),
                    "El municipio no pertenece al departamento seleccionado");
            }
        }

        /// <summary>
        /// Descarta los renglones de "Lo que se entregó" que llegaron vacíos (el usuario
        /// no llenó esa fila) y exige que quede al menos uno con datos.
        /// </summary>
        private void LimpiarDetallesVacios(EntregaProgramada entrega)
        {
            var vacias = entrega.Detalles
                .Select((d, indice) => (d, indice))
                .Where(x => string.IsNullOrWhiteSpace(x.d.Producto))
                .Select(x => x.indice)
                .ToList();

            // Se descartan los errores de validación solo de las filas vacías: ya no
            // forman parte del modelo y no deben bloquear el guardado de las demás.
            foreach (var indice in vacias)
            {
                foreach (var clave in ModelState.Keys.Where(k => k.StartsWith($"Detalles[{indice}].")).ToList())
                {
                    ModelState.Remove(clave);
                }
            }

            entrega.Detalles = entrega.Detalles
                .Where(d => !string.IsNullOrWhiteSpace(d.Producto))
                .ToList();

            if (entrega.Detalles.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un producto en \"Lo que se entregó\"");
            }
        }
    }
}
