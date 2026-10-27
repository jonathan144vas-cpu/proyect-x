// Gráficas de la pantalla "Visualización". Los datos llegan ya agrupados desde el
// servidor en window.datosVisualizacion; aquí solo se dibujan.
(function () {
    var datos = window.datosVisualizacion;
    if (!datos || typeof Chart === 'undefined') return;

    var TEXTO = '#94a3b8';
    var TEXTO_FUERTE = '#f8fafc';
    var SUPERFICIE = '#1e293b';
    var REJILLA = 'rgba(148, 163, 184, 0.14)';

    // Colores categóricos en orden fijo (validados para fondo oscuro).
    var CATEGORICOS = ['#3987e5', '#d95926', '#199e70', '#c98500', '#d55181', '#9085e9'];
    var GRIS = '#64748b';

    // Estados con significado propio: siempre el mismo color, sin importar cuántos haya.
    var COLOR_POR_NOMBRE = {
        'Vigente': '#0ca30c',
        'Por vencer': '#fab219',
        'Vencido': '#d03b3b',
        'Sin vencimiento': GRIS,
        'Programadas': '#3987e5',
        'Completadas': '#0ca30c',
        'Canceladas': GRIS,
        'Pendientes': '#fab219',
        'Solicitados': '#3987e5',
        'Recibidos': '#0ca30c',
        'Cancelados': GRIS,
        'Prioridad alta': '#d03b3b',
        'Prioridad normal': '#3987e5',
        'Prioridad baja': GRIS,
        'Otros': GRIS
    };

    Chart.defaults.font.family = "'Inter', -apple-system, BlinkMacSystemFont, sans-serif";
    Chart.defaults.color = TEXTO;

    var formato = new Intl.NumberFormat('es-GT');

    function colorDe(nombre, indice) {
        return COLOR_POR_NOMBRE[nombre] || CATEGORICOS[indice % CATEGORICOS.length];
    }

    function nombreDeMes(clave, conAnio) {
        var partes = clave.split('-');
        var fecha = new Date(Number(partes[0]), Number(partes[1]) - 1, 1);
        var mes = fecha.toLocaleDateString('es-GT', { month: 'short' }).replace('.', '');
        return conAnio ? mes + ' ' + String(fecha.getFullYear()).slice(2) : mes;
    }

    // Pastel en forma de dona + leyenda en HTML con cantidad y porcentaje,
    // que además sirve como la tabla de datos de la gráfica.
    function dibujarPastel(idLienzo, idLeyenda, puntos) {
        var lienzo = document.getElementById(idLienzo);
        var leyenda = document.getElementById(idLeyenda);
        if (!lienzo || !leyenda) return;

        var total = puntos.reduce(function (suma, p) { return suma + p.valor; }, 0);

        if (puntos.length === 0 || total === 0) {
            lienzo.parentElement.classList.add('grafica-vacia');
            lienzo.remove();
            leyenda.innerHTML = '<li class="leyenda-vacia">Todavía no hay datos para mostrar.</li>';
            return;
        }

        var colores = puntos.map(function (p, i) { return colorDe(p.etiqueta, i); });

        new Chart(lienzo, {
            type: 'doughnut',
            data: {
                labels: puntos.map(function (p) { return p.etiqueta; }),
                datasets: [{
                    data: puntos.map(function (p) { return p.valor; }),
                    backgroundColor: colores,
                    borderColor: SUPERFICIE,
                    borderWidth: 2,
                    hoverOffset: 6
                }]
            },
            options: {
                cutout: '62%',
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        borderColor: '#334155',
                        borderWidth: 1,
                        padding: 10,
                        callbacks: {
                            label: function (contexto) {
                                var porcentaje = Math.round(contexto.parsed * 100 / total);
                                return ' ' + formato.format(contexto.parsed) + ' (' + porcentaje + '%)';
                            }
                        }
                    }
                }
            }
        });

        leyenda.innerHTML = puntos.map(function (p, i) {
            var porcentaje = Math.round(p.valor * 100 / total);
            return '<li><span class="leyenda-color" style="background:' + colores[i] + '"></span>' +
                '<span class="leyenda-nombre">' + p.etiqueta + '</span>' +
                '<span class="leyenda-valor">' + formato.format(p.valor) + ' <small>' + porcentaje + '%</small></span></li>';
        }).join('');
    }

    function dibujarLineas(idLienzo, series) {
        var lienzo = document.getElementById(idLienzo);
        if (!lienzo) return;

        var meses = series[0].puntos.map(function (p) { return p.etiqueta; });

        new Chart(lienzo, {
            type: 'line',
            data: {
                labels: meses.map(function (m) { return nombreDeMes(m, false); }),
                datasets: series.map(function (serie, i) {
                    return {
                        label: serie.nombre,
                        data: serie.puntos.map(function (p) { return p.valor; }),
                        borderColor: CATEGORICOS[i],
                        backgroundColor: CATEGORICOS[i],
                        borderWidth: 2,
                        tension: 0.3,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        pointBorderColor: SUPERFICIE,
                        pointBorderWidth: 2
                    };
                })
            },
            options: {
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                scales: {
                    x: { grid: { display: false }, border: { color: '#334155' } },
                    y: {
                        beginAtZero: true,
                        grid: { color: REJILLA },
                        border: { display: false },
                        ticks: { precision: 0, callback: function (v) { return formato.format(v); } }
                    }
                },
                plugins: {
                    legend: {
                        display: series.length > 1,
                        align: 'end',
                        labels: { usePointStyle: true, boxWidth: 8, boxHeight: 8, color: TEXTO_FUERTE }
                    },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        borderColor: '#334155',
                        borderWidth: 1,
                        padding: 10,
                        callbacks: {
                            title: function (items) { return nombreDeMes(meses[items[0].dataIndex], true); },
                            label: function (contexto) {
                                return ' ' + contexto.dataset.label + ': ' + formato.format(contexto.parsed.y);
                            }
                        }
                    }
                }
            }
        });
    }

    dibujarPastel('graficaCategorias', 'leyendaCategorias', datos.inventarioPorCategoria);
    dibujarPastel('graficaVencimiento', 'leyendaVencimiento', datos.estadoVencimiento);
    dibujarPastel('graficaEnvios', 'leyendaEnvios', datos.enviosPorEstado);
    dibujarPastel('graficaDepartamentos', 'leyendaDepartamentos', datos.enviosPendientesPorDepartamento);
    dibujarPastel('graficaPedidos', 'leyendaPedidos', datos.pedidosPorEstado);
    dibujarPastel('graficaPrioridad', 'leyendaPrioridad', datos.pedidosActivosPorPrioridad);

    dibujarLineas('graficaEntregasMes', [
        { nombre: 'Realizadas', puntos: datos.entregasRealizadasPorMes },
        { nombre: 'Programadas', puntos: datos.entregasProgramadasPorMes }
    ]);
    dibujarLineas('graficaIngresoMes', [
        { nombre: 'Unidades que ingresaron', puntos: datos.ingresoInventarioPorMes }
    ]);
})();
