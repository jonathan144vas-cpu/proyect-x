using ControlViveresApp.Data;
using ControlViveresApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlViveresApp.Controllers
{
    /// <summary>
    /// Estadísticas y gráficas de lo que hay en bodega y de los envíos por hacer.
    /// Es de solo lectura: junta datos de varias tablas y los entrega ya agrupados.
    /// </summary>
    [Authorize]
    public class VisualizacionController : Controller
    {
        private const int DiasParaPorVencer = 7;
        private const int MesesAtras = 11;
        private const int MesesAdelante = 2;
        private const int DepartamentosVisibles = 5;

        private readonly ContextoViveres _contexto;

        public VisualizacionController(ContextoViveres contexto)
        {
            _contexto = contexto;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var alimentos = await _contexto.Alimentos.AsNoTracking().ToListAsync();
            var programadas = await _contexto.EntregasProgramadas.AsNoTracking()
                .Where(e => e.Estado == EstadoEntregaProgramada.Programada)
                .ToListAsync();
            var realizadas = await _contexto.Entregas.AsNoTracking()
                .Select(e => e.FechaEntrega)
                .ToListAsync();
            var pedidos = await _contexto.Pedidos.AsNoTracking()
                .Select(p => new { p.Estado, p.Prioridad })
                .ToListAsync();
            var conteoEnvios = await _contexto.EntregasProgramadas.AsNoTracking()
                .GroupBy(e => e.Estado)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToListAsync();

            var modelo = new VisualizacionViewModel
            {
                UnidadesEnBodega = alimentos.Sum(a => a.Cantidad),
                ProductosDistintos = alimentos.Select(a => a.Nombre.Trim().ToLower()).Distinct().Count(),
                UnidadesPorVencer = alimentos.Where(a => EstadoDeVencimiento(a, hoy) == "Por vencer").Sum(a => a.Cantidad),
                EnviosPorHacer = programadas.Count,
                FamiliasPorAtender = programadas.Sum(e => e.FamiliasBeneficiadas),
                PedidosActivos = pedidos.Count(p => EsActivo(p.Estado))
            };

            modelo.InventarioPorCategoria = alimentos
                .GroupBy(a => a.Categoria)
                .Select(g => new Punto(g.Key, g.Sum(a => a.Cantidad)))
                .OrderByDescending(p => p.Valor)
                .ToList();

            modelo.EstadoVencimiento = alimentos
                .GroupBy(a => EstadoDeVencimiento(a, hoy))
                .Select(g => new Punto(g.Key, g.Sum(a => a.Cantidad)))
                .OrderBy(p => OrdenVencimiento.IndexOf(p.Etiqueta))
                .ToList();

            modelo.EnviosPorEstado = conteoEnvios
                .OrderBy(c => c.Estado)
                .Select(c => new Punto(NombreDe(c.Estado), c.Total))
                .ToList();

            modelo.EnviosPendientesPorDepartamento = AgruparConOtros(
                programadas.GroupBy(e => e.Departamento)
                           .Select(g => new Punto(g.Key, g.Count()))
                           .OrderByDescending(p => p.Valor)
                           .ToList());

            modelo.PedidosPorEstado = pedidos
                .GroupBy(p => p.Estado)
                .OrderBy(g => g.Key)
                .Select(g => new Punto(NombreDe(g.Key), g.Count()))
                .ToList();

            modelo.PedidosActivosPorPrioridad = pedidos
                .Where(p => EsActivo(p.Estado))
                .GroupBy(p => p.Prioridad)
                .OrderByDescending(g => g.Key)
                .Select(g => new Punto($"Prioridad {g.Key.ToString().ToLower()}", g.Count()))
                .ToList();

            var meses = UltimosMeses(hoy);

            modelo.EntregasRealizadasPorMes = meses
                .Select(mes => new Punto(mes, realizadas.Count(f => ClaveMes(f) == mes)))
                .ToList();

            modelo.EntregasProgramadasPorMes = meses
                .Select(mes => new Punto(mes, programadas.Count(e => ClaveMes(e.FechaProgramada) == mes)))
                .ToList();

            modelo.IngresoInventarioPorMes = meses
                .Select(mes => new Punto(mes, alimentos
                    .Where(a => ClaveMes(DateOnly.FromDateTime(a.FechaRegistro)) == mes)
                    .Sum(a => a.Cantidad)))
                .ToList();

            return View(modelo);
        }

        private static readonly List<string> OrdenVencimiento = ["Vigente", "Por vencer", "Vencido", "Sin vencimiento"];

        private static string EstadoDeVencimiento(Alimento alimento, DateOnly hoy)
        {
            if (alimento.FechaVencimiento is not DateOnly vencimiento) return "Sin vencimiento";
            if (vencimiento < hoy) return "Vencido";
            if (vencimiento.DayNumber - hoy.DayNumber <= DiasParaPorVencer) return "Por vencer";
            return "Vigente";
        }

        private static bool EsActivo(EstadoPedido estado) =>
            estado is EstadoPedido.Pendiente or EstadoPedido.Solicitado;

        private static string NombreDe(EstadoEntregaProgramada estado) => estado switch
        {
            EstadoEntregaProgramada.Programada => "Programadas",
            EstadoEntregaProgramada.Completada => "Completadas",
            _ => "Canceladas"
        };

        private static string NombreDe(EstadoPedido estado) => estado switch
        {
            EstadoPedido.Pendiente => "Pendientes",
            EstadoPedido.Solicitado => "Solicitados",
            EstadoPedido.Recibido => "Recibidos",
            _ => "Cancelados"
        };

        /// <summary>Deja los primeros departamentos y suma el resto en "Otros" para que el pastel se lea bien.</summary>
        private static List<Punto> AgruparConOtros(List<Punto> puntos)
        {
            if (puntos.Count <= DepartamentosVisibles) return puntos;

            var visibles = puntos.Take(DepartamentosVisibles).ToList();
            visibles.Add(new Punto("Otros", puntos.Skip(DepartamentosVisibles).Sum(p => p.Valor)));
            return visibles;
        }

        private static string ClaveMes(DateOnly fecha) => $"{fecha.Year:0000}-{fecha.Month:00}";

        private static List<string> UltimosMeses(DateOnly hoy)
        {
            var primero = new DateOnly(hoy.Year, hoy.Month, 1).AddMonths(-MesesAtras);

            return Enumerable.Range(0, MesesAtras + MesesAdelante + 1)
                .Select(i => ClaveMes(primero.AddMonths(i)))
                .ToList();
        }
    }
}
