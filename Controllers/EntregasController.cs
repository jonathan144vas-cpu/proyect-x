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
    /// <summary>
    /// Registro de los lugares de Guatemala donde ya se entregaron víveres.
    /// </summary>
    [Authorize]
    public class EntregasController : Controller
    {
        private readonly ContextoViveres _contexto;

        public EntregasController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        // 1. LISTAR LUGARES VISITADOS
        public async Task<IActionResult> Index(string? buscar, string? departamento)
        {
            var consulta = _contexto.Entregas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                consulta = consulta.Where(e => e.Lugar.ToLower().Contains(texto)
                                            || (e.Municipio != null && e.Municipio.ToLower().Contains(texto)));
            }

            if (!string.IsNullOrWhiteSpace(departamento))
            {
                consulta = consulta.Where(e => e.Departamento == departamento);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Departamento = departamento;

            // Los totales se calculan sobre TODO, no sobre lo filtrado.
            ViewBag.TotalLugares = await _contexto.Entregas.CountAsync();
            ViewBag.TotalFamilias = await _contexto.Entregas.SumAsync(e => e.FamiliasBeneficiadas);
            ViewBag.TotalEntregado = await _contexto.Entregas.SumAsync(e => e.TotalEntregado);
            ViewBag.TotalDepartamentos = await _contexto.Entregas
                .Select(e => e.Departamento).Distinct().CountAsync();

            var lista = await consulta
                .OrderByDescending(e => e.FechaEntrega)
                .ThenByDescending(e => e.Id)
                .ToListAsync();

            return View(lista);
        }

        // 2. NUEVO (PANTALLA)
        [Authorize(Roles = "Admin")]
        public IActionResult Crear()
        {
            return View(new Entrega
            {
                FechaEntrega = DateOnly.FromDateTime(DateTime.Now)
            });
        }

        // 3. GUARDAR NUEVO
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(
            [Bind("Lugar,Departamento,Municipio,FechaEntrega,FamiliasBeneficiadas,TotalEntregado,Latitud,Longitud,Observaciones")] Entrega entrega)
        {
            ValidarDepartamento(entrega);

            if (!ModelState.IsValid)
            {
                return View(entrega);
            }

            AsignarCoordenadas(entrega);
            entrega.RegistradoPor = User.Identity?.Name;
            entrega.FechaRegistro = DateTime.UtcNow;

            _contexto.Entregas.Add(entrega);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Se registró la entrega en {entrega.Lugar}";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var entrega = await _contexto.Entregas.FindAsync(id);
            if (entrega is null) return NotFound();

            return View(entrega);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Lugar,Departamento,Municipio,FechaEntrega,FamiliasBeneficiadas,TotalEntregado,Latitud,Longitud,Observaciones")] Entrega entrega)
        {
            if (id != entrega.Id) return NotFound();

            ValidarDepartamento(entrega);

            if (!ModelState.IsValid)
            {
                return View(entrega);
            }

            var existente = await _contexto.Entregas.FindAsync(id);
            if (existente is null) return NotFound();

            AsignarCoordenadas(entrega);

            // RegistradoPor y FechaRegistro no se tocan: son el historial.
            existente.Lugar = entrega.Lugar;
            existente.Departamento = entrega.Departamento;
            existente.Municipio = entrega.Municipio;
            existente.FechaEntrega = entrega.FechaEntrega;
            existente.FamiliasBeneficiadas = entrega.FamiliasBeneficiadas;
            existente.TotalEntregado = entrega.TotalEntregado;
            existente.Latitud = entrega.Latitud;
            existente.Longitud = entrega.Longitud;
            existente.Observaciones = entrega.Observaciones;

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Entrega actualizada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 6. ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var entrega = await _contexto.Entregas.FindAsync(id);

            if (entrega is null)
            {
                TempData["Mensaje"] = "Esa entrega ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _contexto.Entregas.Remove(entrega);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Entrega eliminada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private void ValidarDepartamento(Entrega entrega)
        {
            if (!string.IsNullOrWhiteSpace(entrega.Departamento) &&
                Catalogos.BuscarDepartamento(entrega.Departamento) is null)
            {
                ModelState.AddModelError(nameof(Entrega.Departamento),
                    "El departamento seleccionado no es de Guatemala");
            }
        }

        /// <summary>
        /// Si no se escribieron coordenadas a mano, se toman las de la cabecera
        /// del departamento para que el punto pueda salir en el mapa.
        /// </summary>
        private static void AsignarCoordenadas(Entrega entrega)
        {
            if (entrega.Latitud is not null && entrega.Longitud is not null) return;

            var departamento = Catalogos.BuscarDepartamento(entrega.Departamento);
            if (departamento is null) return;

            entrega.Latitud ??= departamento.Latitud;
            entrega.Longitud ??= departamento.Longitud;
        }
    }
}
