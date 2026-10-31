// Gráficas de la pantalla "Visualización". Los datos llegan ya agrupados desde el servidor
// en window.datosVisualizacion; aquí solo se dibujan. Cada cierto tiempo se vuelven a pedir
// al servidor y las gráficas se actualizan en su lugar, sin recargar la página.
(function () {
    var panel = document.getElementById('panelVisualizacion');
    if (!window.datosVisualizacion || !panel || typeof Chart === 'undefined') return;

    var INTERVALO = 30000; // milisegundos entre una actualización y la siguiente

    // Colores de texto, fondo y rejilla: salen de las variables del tema activo.
    var TEXTO, TEXTO_FUERTE, SUPERFICIE, REJILLA, FONDO_TIP, BORDE;
    function leerTema() {
        var estilo = getComputedStyle(document.documentElement);
        function v(n) { return estilo.getPropertyValue(n).trim(); }
        var claro = document.documentElement.getAttribute('data-tema') === 'claro';
        TEXTO = v('--text-muted');
        TEXTO_FUERTE = v('--text-main');
        SUPERFICIE = v('--bg-card');
        FONDO_TIP = v('--bg-card');
        BORDE = v('--border');
        REJILLA = claro ? 'rgba(100, 116, 139, 0.22)' : 'rgba(148, 163, 184, 0.14)';
        Chart.defaults.color = TEXTO;
    }

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
    leerTema();

    var formato = new Intl.NumberFormat('es-GT');
    var graficas = {}; // id del lienzo -> gráfica ya creada

    function colorDe(nombre, indice) {
        return COLOR_POR_NOMBRE[nombre] || CATEGORICOS[indice % CATEGORICOS.length];
    }

    function nombreDeMes(clave, conAnio) {
        var partes = clave.split('-');
        var fecha = new Date(Number(partes[0]), Number(partes[1]) - 1, 1);
        var mes = fecha.toLocaleDateString('es-GT', { month: 'short' }).replace('.', '');
        return conAnio ? mes + ' ' + String(fecha.getFullYear()).slice(2) : mes;
    }

    function itemDeLeyenda(color, nombre, valor, porcentaje) {
        var item = document.createElement('li');

        var muestra = document.createElement('span');
        muestra.className = 'leyenda-color';
        muestra.style.background = color;

        var etiqueta = document.createElement('span');
        etiqueta.className = 'leyenda-nombre';
        etiqueta.textContent = nombre;

        var cantidad = document.createElement('span');
        cantidad.className = 'leyenda-valor';
        cantidad.textContent = formato.format(valor) + ' ';
        var pequeno = document.createElement('small');
        pequeno.textContent = porcentaje + '%';
        cantidad.appendChild(pequeno);

        item.appendChild(muestra);
        item.appendChild(etiqueta);
        item.appendChild(cantidad);
        return item;
    }

    // Pastel en forma de dona + leyenda en HTML con cantidad y porcentaje,
    // que además sirve como la tabla de datos de la gráfica.
    function dibujarPastel(idLienzo, idLeyenda, puntos) {
        var lienzo = document.getElementById(idLienzo);
        var leyenda = document.getElementById(idLeyenda);
        if (!lienzo || !leyenda) return;

        var total = puntos.reduce(function (suma, p) { return suma + p.valor; }, 0);
        leyenda.innerHTML = '';

        if (puntos.length === 0 || total === 0) {
            lienzo.style.display = 'none';
            lienzo.parentElement.classList.add('grafica-vacia');
            var vacio = document.createElement('li');
            vacio.className = 'leyenda-vacia';
            vacio.textContent = 'Todavía no hay datos para mostrar.';
            leyenda.appendChild(vacio);
            return;
        }

        lienzo.style.display = 'block';
        lienzo.parentElement.classList.remove('grafica-vacia');

        var colores = puntos.map(function (p, i) { return colorDe(p.etiqueta, i); });
        var etiquetas = puntos.map(function (p) { return p.etiqueta; });
        var valores = puntos.map(function (p) { return p.valor; });

        var existente = graficas[idLienzo];
        if (existente) {
            existente.data.labels = etiquetas;
            existente.data.datasets[0].data = valores;
            existente.data.datasets[0].backgroundColor = colores;
            existente.update();
        } else {
            graficas[idLienzo] = new Chart(lienzo, {
                type: 'doughnut',
                data: {
                    labels: etiquetas,
                    datasets: [{
                        data: valores,
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
                            backgroundColor: FONDO_TIP,
                            borderColor: BORDE,
                            titleColor: TEXTO_FUERTE,
                            bodyColor: TEXTO_FUERTE,
                            borderWidth: 1,
                            padding: 10,
                            callbacks: {
                                label: function (contexto) {
                                    var suma = contexto.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                    var porcentaje = Math.round(contexto.parsed * 100 / suma);
                                    return ' ' + formato.format(contexto.parsed) + ' (' + porcentaje + '%)';
                                }
                            }
                        }
                    }
                }
            });
        }

        puntos.forEach(function (p, i) {
            leyenda.appendChild(itemDeLeyenda(colores[i], p.etiqueta, p.valor, Math.round(p.valor * 100 / total)));
        });
    }

    function dibujarLineas(idLienzo, series) {
        var lienzo = document.getElementById(idLienzo);
        if (!lienzo) return;

        var meses = series[0].puntos.map(function (p) { return p.etiqueta; });
        var etiquetas = meses.map(function (m) { return nombreDeMes(m, false); });

        var existente = graficas[idLienzo];
        if (existente) {
            existente.$meses = meses;
            existente.data.labels = etiquetas;
            series.forEach(function (serie, i) {
                existente.data.datasets[i].data = serie.puntos.map(function (p) { return p.valor; });
            });
            existente.update();
            return;
        }

        var grafica = new Chart(lienzo, {
            type: 'line',
            data: {
                labels: etiquetas,
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
                    x: { grid: { display: false }, border: { color: BORDE } },
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
                        backgroundColor: FONDO_TIP,
                        borderColor: BORDE,
                        titleColor: TEXTO_FUERTE,
                        bodyColor: TEXTO_FUERTE,
                        borderWidth: 1,
                        padding: 10,
                        callbacks: {
                            title: function (items) { return nombreDeMes(grafica.$meses[items[0].dataIndex], true); },
                            label: function (contexto) {
                                return ' ' + contexto.dataset.label + ': ' + formato.format(contexto.parsed.y);
                            }
                        }
                    }
                }
            }
        });

        grafica.$meses = meses;
        graficas[idLienzo] = grafica;
    }

    function mostrarTarjetas(datos) {
        document.querySelectorAll('[data-kpi]').forEach(function (tarjeta) {
            var valor = datos[tarjeta.dataset.kpi];
            if (valor !== undefined) tarjeta.textContent = formato.format(valor);
        });
        document.getElementById('ultimaActualizacion').textContent = datos.actualizadoEn;
    }

    var ultimosDatos = null;

    function aplicar(datos) {
        ultimosDatos = datos;
        mostrarTarjetas(datos);

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
        dibujarLineas('graficaMovimientoMes', [
            { nombre: 'Entradas', puntos: datos.entradasPorMes },
            { nombre: 'Salidas', puntos: datos.salidasPorMes }
        ]);
    }

    // ---------- Actualización automática ----------

    var punto = document.getElementById('puntoActualizacion');
    var pidiendo = false;

    function actualizar() {
        if (pidiendo || document.hidden) return;
        pidiendo = true;

        fetch(panel.dataset.url, { credentials: 'same-origin', cache: 'no-store', headers: { 'Accept': 'application/json' } })
            .then(function (respuesta) {
                if (!respuesta.ok) throw new Error('respuesta ' + respuesta.status);
                return respuesta.json();
            })
            .then(function (datos) {
                aplicar(datos);
                punto.classList.remove('error');
            })
            .catch(function () {
                // Sesión vencida o sin conexión: se deja lo que ya se ve y se avisa con el punto rojo.
                punto.classList.add('error');
            })
            .then(function () { pidiendo = false; });
    }

    aplicar(window.datosVisualizacion);

    // Al cambiar de tema se vuelven a crear las gráficas con los colores nuevos.
    document.addEventListener('tema-cambiado', function () {
        Object.keys(graficas).forEach(function (id) { graficas[id].destroy(); });
        graficas = {};
        leerTema();
        if (ultimosDatos) aplicar(ultimosDatos);
    });

    setInterval(actualizar, INTERVALO);
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) actualizar();
    });
})();
