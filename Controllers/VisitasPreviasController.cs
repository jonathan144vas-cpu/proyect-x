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
    /// Revisión previa de posibles visitas a un lugar, antes de programarlas
    /// formalmente. Al aprobar una visita se genera una Entrega Programada
    /// con los mismos datos y productos propuestos.
    /// </summary>
    [Authorize]
    public class VisitasPreviasController : Controller
    {
        private readonly ContextoViveres _contexto;

        public VisitasPreviasController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        // 1. LISTAR
        public async Task<IActionResult> Index(string? buscar, string? departamento, EstadoVisitaPrevia? estado)
        {
            IQueryable<VisitaPrevia> consulta = _contexto.VisitasPrevias.AsNoTracking().Include(v => v.Detalles);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                consulta = consulta.Where(v => v.Lugar.ToLower().Contains(texto)
                                            || v.Municipio.ToLower().Contains(texto));
            }

            if (!string.IsNullOrWhiteSpace(departamento))
            {
                consulta = consulta.Where(v => v.Departamento == departamento);
            }

            if (estado is not null)
            {
                consulta = consulta.Where(v => v.Estado == estado);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Departamento = departamento;
            ViewBag.Estado = estado;

            ViewBag.TotalPendientes = await _contexto.VisitasPrevias
                .CountAsync(v => v.Estado == EstadoVisitaPrevia.Pendiente);
            ViewBag.TotalAprobadas = await _contexto.VisitasPrevias
                .CountAsync(v => v.Estado == EstadoVisitaPrevia.Aprobada);

            var lista = await consulta
                .OrderBy(v => v.Estado == EstadoVisitaPrevia.Pendiente ? 0 : 1)
                .ThenBy(v => v.FechaPropuesta)
                .ThenByDescending(v => v.Id)
                .ToListAsync();

            return View(lista);
        }

        // 2. CREAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public IActionResult Crear()
        {
            var visita = new VisitaPrevia
            {
                FechaPropuesta = DateOnly.FromDateTime(DateTime.Now)
            };
            visita.Detalles.Add(new DetalleVisitaPrevia());

            return View(visita);
        }

        // 3. GUARDAR NUEVA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear(
            [Bind("Lugar,Departamento,Municipio,FechaPropuesta,FamiliasBeneficiadas,Observaciones,Detalles")] VisitaPrevia visita)
        {
            ValidarUbicacion(visita);
            LimpiarDetallesVacios(visita);

            if (!ModelState.IsValid)
            {
                return View(visita);
            }

            visita.RegistradoPor = User.Identity?.Name;
            visita.FechaRegistro = DateTime.UtcNow;
            visita.Estado = EstadoVisitaPrevia.Pendiente;

            _contexto.VisitasPrevias.Add(visita);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Se registró la visita previa a {visita.Lugar}";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 4. EDITAR (PANTALLA)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id is null) return NotFound();

            var visita = await _contexto.VisitasPrevias
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita is null) return NotFound();
            if (visita.Detalles.Count == 0) visita.Detalles.Add(new DetalleVisitaPrevia());

            return View(visita);
        }

        // 5. GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Id,Lugar,Departamento,Municipio,FechaPropuesta,FamiliasBeneficiadas,Observaciones,Detalles")] VisitaPrevia visita)
        {
            if (id != visita.Id) return NotFound();

            ValidarUbicacion(visita);
            LimpiarDetallesVacios(visita);

            if (!ModelState.IsValid)
            {
                return View(visita);
            }

            var existente = await _contexto.VisitasPrevias
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existente is null) return NotFound();

            if (existente.Estado != EstadoVisitaPrevia.Pendiente)
            {
                TempData["Mensaje"] = "Solo se pueden editar visitas que siguen pendientes";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            existente.Lugar = visita.Lugar;
            existente.Departamento = visita.Departamento;
            existente.Municipio = visita.Municipio;
            existente.FechaPropuesta = visita.FechaPropuesta;
            existente.FamiliasBeneficiadas = visita.FamiliasBeneficiadas;
            existente.Observaciones = visita.Observaciones;

            _contexto.DetallesVisitaPrevia.RemoveRange(existente.Detalles);
            existente.Detalles = visita.Detalles
                .Select(d => new DetalleVisitaPrevia
                {
                    Producto = d.Producto,
                    SistemaMedida = d.SistemaMedida,
                    Total = d.Total
                })
                .ToList();

            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Visita previa actualizada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 6. APROBAR (genera la Entrega Programada correspondiente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Aprobar(int id)
        {
            var visita = await _contexto.VisitasPrevias
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita is null)
            {
                TempData["Mensaje"] = "Esa visita ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (visita.Estado != EstadoVisitaPrevia.Pendiente)
            {
                TempData["Mensaje"] = "Esa visita ya no está pendiente";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index));
            }

            var entregaProgramada = new EntregaProgramada
            {
                Lugar = visita.Lugar,
                Departamento = visita.Departamento,
                Municipio = visita.Municipio,
                FechaProgramada = visita.FechaPropuesta,
                FamiliasBeneficiadas = visita.FamiliasBeneficiadas,
                Observaciones = visita.Observaciones,
                RegistradoPor = User.Identity?.Name,
                FechaRegistro = DateTime.UtcNow,
                Estado = EstadoEntregaProgramada.Programada,
                Detalles = visita.Detalles.Select(d => new DetalleEntregaProgramada
                {
                    Producto = d.Producto,
                    SistemaMedida = d.SistemaMedida,
                    Total = d.Total
                }).ToList()
            };
            _contexto.EntregasProgramadas.Add(entregaProgramada);

            visita.Estado = EstadoVisitaPrevia.Aprobada;
            visita.FechaRevision = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();

            visita.EntregaProgramadaGeneradaId = entregaProgramada.Id;
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = $"Visita a {visita.Lugar} aprobada. Se programó como entrega con los productos propuestos.";
            TempData["Tipo"] = "success";

            // Se manda directo a Entregas Programadas, ya con los productos cargados.
            return RedirectToAction("Editar", "EntregasProgramadas", new { id = entregaProgramada.Id });
        }

        // 7. RECHAZAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Rechazar(int id)
        {
            var visita = await _contexto.VisitasPrevias.FindAsync(id);

            if (visita is null || visita.Estado != EstadoVisitaPrevia.Pendiente)
            {
                TempData["Mensaje"] = "Esa visita ya no se puede rechazar";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            visita.Estado = EstadoVisitaPrevia.Rechazada;
            visita.FechaRevision = DateTime.UtcNow;
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Visita previa rechazada";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // 8. ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var visita = await _contexto.VisitasPrevias.FindAsync(id);

            if (visita is null)
            {
                TempData["Mensaje"] = "Esa visita ya no existe";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (visita.Estado == EstadoVisitaPrevia.Aprobada)
            {
                TempData["Mensaje"] = "No se puede eliminar una visita ya aprobada";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _contexto.VisitasPrevias.Remove(visita);
            await _contexto.SaveChangesAsync();

            TempData["Mensaje"] = "Visita previa eliminada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private void ValidarUbicacion(VisitaPrevia visita)
        {
            if (!string.IsNullOrWhiteSpace(visita.Departamento) &&
                Catalogos.BuscarDepartamento(visita.Departamento) is null)
            {
                ModelState.AddModelError(nameof(VisitaPrevia.Departamento),
                    "El departamento seleccionado no es de Guatemala");
                return;
            }

            if (!string.IsNullOrWhiteSpace(visita.Municipio) &&
                !Catalogos.MunicipiosDe(visita.Departamento).Contains(visita.Municipio))
            {
                ModelState.AddModelError(nameof(VisitaPrevia.Municipio),
                    "El municipio no pertenece al departamento seleccionado");
            }
        }

        /// <summary>
        /// Descarta los renglones de productos propuestos que llegaron vacíos y exige
        /// que quede al menos uno con datos.
        /// </summary>
        private void LimpiarDetallesVacios(VisitaPrevia visita)
        {
            var vacias = visita.Detalles
                .Select((d, indice) => (d, indice))
                .Where(x => string.IsNullOrWhiteSpace(x.d.Producto))
                .Select(x => x.indice)
                .ToList();

            foreach (var indice in vacias)
            {
                foreach (var clave in ModelState.Keys.Where(k => k.StartsWith($"Detalles[{indice}].")).ToList())
                {
                    ModelState.Remove(clave);
                }
            }

            visita.Detalles = visita.Detalles
                .Where(d => !string.IsNullOrWhiteSpace(d.Producto))
                .ToList();

            if (visita.Detalles.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un producto propuesto");
            }
        }
    }
}
