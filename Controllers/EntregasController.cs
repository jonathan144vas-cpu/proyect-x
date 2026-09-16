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
    /// Historial de solo lectura de los lugares de Guatemala donde ya se entregaron
    /// víveres. Se alimenta automáticamente al completar una entrega programada
    /// (ver EntregasProgramadasController). No tiene pantallas de alta/edición.
    /// </summary>
    [Authorize]
    public class EntregasController : Controller
    {
        private readonly ContextoViveres _contexto;

        public EntregasController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        // LISTAR LUGARES VISITADOS
        public async Task<IActionResult> Index(string? buscar, string? departamento)
        {
            IQueryable<Entrega> consulta = _contexto.Entregas.AsNoTracking().Include(e => e.Detalles);

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

    }
}
