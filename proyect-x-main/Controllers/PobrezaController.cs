using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    /// <summary>
    /// Mapa de pobreza de Guatemala (ENCOVI 2023) con lo que la campaña ha hecho en cada
    /// departamento, para ver si la ayuda está llegando donde más se necesita.
    /// </summary>
    [Authorize]
    public class PobrezaController : Controller
    {
        private readonly ContextoViveres _contexto;

        public PobrezaController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        public async Task<IActionResult> Index()
        {
            var entregas = await _contexto.Entregas.AsNoTracking()
                .GroupBy(e => e.Departamento)
                .Select(g => new { Departamento = g.Key, Total = g.Count(), Familias = g.Sum(e => e.FamiliasBeneficiadas) })
                .ToListAsync();

            var pendientes = await _contexto.EntregasProgramadas.AsNoTracking()
                .Where(e => e.Estado == EstadoEntregaProgramada.Programada)
                .GroupBy(e => e.Departamento)
                .Select(g => new { Departamento = g.Key, Total = g.Count() })
                .ToListAsync();

            var departamentos = DatosPobreza.PorDepartamento
                .Select(d => new DepartamentoPobreza(
                    d.Departamento,
                    d.PobrezaTotal2023,
                    d.PobrezaExtrema2023,
                    d.PobrezaTotal2014,
                    d.PobrezaExtrema2014,
                    d.PobrezaMultidimensional2023,
                    entregas.FirstOrDefault(e => e.Departamento == d.Departamento)?.Total ?? 0,
                    entregas.FirstOrDefault(e => e.Departamento == d.Departamento)?.Familias ?? 0,
                    pendientes.FirstOrDefault(e => e.Departamento == d.Departamento)?.Total ?? 0))
                .ToList();

            return View(departamentos);
        }
    }
}
