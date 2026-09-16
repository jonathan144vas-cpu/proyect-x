using System;
using System.Linq;
using System.Threading.Tasks;
using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ControlViveresApp.Controllers
{
    [Authorize]
    public class AlimentosController : Controller
    {
        private readonly ContextoViveres _contexto;
        private readonly OpcionesCampania _campania;

        public AlimentosController(ContextoViveres contexto, IOptions<OpcionesCampania> campania)
        {
            _contexto = contexto;
            _campania = campania.Value;
        }

        // 1. LISTAR ALIMENTOS
        public async Task<IActionResult> Index(string? buscar, string? categoria)
        {
            var consulta = _contexto.Alimentos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                consulta = consulta.Where(a => a.Nombre.ToLower().Contains(texto));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                consulta = consulta.Where(a => a.Categoria == categoria);
            }

            // Se devuelven los filtros a la vista para que el formulario los conserve.
            ViewBag.Buscar = buscar;
            ViewBag.Categoria = categoria;

            // El total recaudado se calcula sobre TODO el inventario, no sobre lo filtrado:
            // la meta de la campaña no cambia porque el usuario escriba en el buscador.
            var totalRecaudado = await _contexto.Alimentos.SumAsync(a => a.Cantidad);

            ViewBag.Campania = _campania;
            ViewBag.TotalRecaudado = totalRecaudado;
            ViewBag.PorcentajeMeta = _campania.PorcentajeDe(totalRecaudado);

            // Los que están más próximos a vencer aparecen primero; los que no tienen
            // fecha de vencimiento (no perecederos) quedan al final.
            var listaAlimentos = await consulta
                .OrderBy(a => a.FechaVencimiento == null ? 1 : 0)
                .ThenBy(a => a.FechaVencimiento)
                .ThenByDescending(a => a.Id)
                .ToListAsync();

            return View(listaAlimentos);
        }

        // 1B. PRODUCTOS SEPARADOS: un cuadro por cada producto del inventario, con
        // todos sus lotes ordenados ascendente por fecha de vencimiento. Se calcula
        // en vivo desde el inventario, así que siempre queda sincronizado: un producto
        // nuevo genera su propio cuadro automáticamente.
        public async Task<IActionResult> ProductosSeparados()
        {
            var alimentos = await _contexto.Alimentos.AsNoTracking().ToListAsync();

            var grupos = alimentos
                .GroupBy(a => a.Nombre)
                .Select(g => new ProductoSeparadoViewModel
                {
                    Nombre = g.Key,
                    Lotes = g.OrderBy(a => a.FechaVencimiento == null ? 1 : 0)
                             .ThenBy(a => a.FechaVencimiento)
                             .ToList()
                })
                // Los productos con el lote más urgente (más próximo a vencer o ya vencido)
                // aparecen primero; los que no tienen fecha de vencimiento van al final.
                .OrderBy(g => g.Lotes.Min(a => a.FechaVencimiento) ?? DateOnly.MaxValue)
                .ThenBy(g => g.Nombre)
                .ToList();

            return View(grupos);
        }

        // 1C. DETALLE DE UN PRODUCTO: todos sus lotes, sin filtrar por rango de días
        public async Task<IActionResult> Detalle(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return NotFound();
            }

            var normalizado = nombre.Trim().ToLower();

            var lotes = await _contexto.Alimentos.AsNoTracking()
                .Where(a => a.Nombre.ToLower() == normalizado)
                .OrderBy(a => a.FechaVencimiento == null ? 1 : 0)
                .ThenBy(a => a.FechaVencimiento)
                .ToListAsync();

            if (lotes.Count == 0)
            {
                return NotFound();
            }

            ViewBag.Nombre = nombre;
            return View(lotes);
        }

        // 2. CREAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public IActionResult Crear()
        {
            return View(new Alimento());
        }

        // 3. GUARDAR NUEVO ALIMENTO
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(
            [Bind("Nombre,Categoria,Cantidad,UnidadMedida,FechaVencimiento")] Alimento alimento)
        {
            ValidarCatalogos(alimento);

            if (!ModelState.IsValid)
            {
                return View(alimento);
            }

            alimento.FechaRegistro = DateTime.UtcNow;

            _contexto.Alimentos.Add(alimento);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Alimento agregado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var alimento = await _contexto.Alimentos.FindAsync(id);
            if (alimento is null) return NotFound();

            return View(alimento);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Nombre,Categoria,Cantidad,UnidadMedida,FechaVencimiento")] Alimento alimento)
        {
            if (id != alimento.Id) return NotFound();

            ValidarCatalogos(alimento);

            if (!ModelState.IsValid)
            {
                return View(alimento);
            }

            // Se carga el registro real y solo se cambian los campos del formulario.
            // Así FechaRegistro conserva la fecha en que el alimento entró al inventario.
            var existente = await _contexto.Alimentos.FindAsync(id);
            if (existente is null) return NotFound();

            existente.Nombre = alimento.Nombre;
            existente.Categoria = alimento.Categoria;
            existente.Cantidad = alimento.Cantidad;
            existente.UnidadMedida = alimento.UnidadMedida;
            existente.FechaVencimiento = alimento.FechaVencimiento;

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Alimento actualizado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 6. ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var alimento = await _contexto.Alimentos.FindAsync(id);

            if (alimento is null)
            {
                TempData["Mensaje"] = "El alimento ya no existe en el inventario";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _contexto.Alimentos.Remove(alimento);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Alimento eliminado correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Comprueba que la categoría y la unidad vengan de las listas permitidas.
        /// Sin esto, alguien podría enviar el formulario con valores inventados.
        /// </summary>
        private void ValidarCatalogos(Alimento alimento)
        {
            if (!string.IsNullOrWhiteSpace(alimento.Categoria) &&
                !Catalogos.Categorias.Contains(alimento.Categoria))
            {
                ModelState.AddModelError(nameof(Alimento.Categoria), "La categoría seleccionada no es válida");
            }

            if (!string.IsNullOrWhiteSpace(alimento.UnidadMedida) &&
                !Catalogos.UnidadesMedida.Contains(alimento.UnidadMedida))
            {
                ModelState.AddModelError(nameof(Alimento.UnidadMedida), "La unidad de medida seleccionada no es válida");
            }
        }
    }
}
