// Mapa de pobreza de Guatemala por departamento. Los datos llegan del servidor en
// window.datosPobreza y los límites de los departamentos se leen de departamentos.json.
(function () {
    var datos = window.datosPobreza;
    var contenedorMapa = document.getElementById('mapaPobreza');
    if (!datos || !contenedorMapa) return;

    var NS = 'http://www.w3.org/2000/svg';

    // Cinco rangos por indicador, de menos a más pobreza. Cada rango lleva un tono
    // de la misma gama azul: más claro significa más pobreza.
    var COLORES = ['#104281', '#1c5cab', '#2a78d6', '#6da7ec', '#b7d3f6'];
    var INDICADORES = {
        general: {
            campo: 'pobrezaTotal2023',
            titulo: 'Pobreza general',
            fuente: 'ENCOVI 2023',
            cortes: [40, 55, 70, 80]
        },
        extrema: {
            campo: 'pobrezaExtrema2023',
            titulo: 'Pobreza extrema',
            fuente: 'ENCOVI 2023',
            cortes: [5, 10, 20, 35]
        },
        multidimensional: {
            campo: 'pobrezaMultidimensional2023',
            titulo: 'Pobreza multidimensional',
            fuente: 'IPM-Gt, informe de 2025',
            cortes: [30, 50, 65, 75]
        }
    };

    var indicadorActual = 'general';
    var departamentoElegido = null;
    var mostrarEntregas = false;

    var porNombre = {};
    datos.forEach(function (d) { porNombre[d.nombre] = d; });

    var caminos = {};      // nombre -> <path>
    var puntos = {};       // nombre -> <circle>
    var filasRanking = {}; // nombre -> <li>

    var tooltip = document.getElementById('mapaTooltip');
    var panelDetalle = document.getElementById('panelDetalle');
    var leyenda = document.getElementById('mapaLeyenda');
    var ranking = document.getElementById('rankingDepartamentos');
    var tituloMapa = document.getElementById('tituloMapa');

    function claseDe(valor) {
        var cortes = INDICADORES[indicadorActual].cortes;
        for (var i = 0; i < cortes.length; i++) {
            if (valor < cortes[i]) return i;
        }
        return cortes.length;
    }

    function colorDe(departamento) {
        return COLORES[claseDe(departamento[INDICADORES[indicadorActual].campo])];
    }

    function porcentaje(valor) {
        return valor.toLocaleString('es-GT', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + '%';
    }

    function crear(nombre, atributos) {
        var elemento = document.createElementNS(NS, nombre);
        Object.keys(atributos || {}).forEach(function (k) { elemento.setAttribute(k, atributos[k]); });
        return elemento;
    }

    // ---------- Dibujo del mapa ----------

    // Proyección simple: la longitud se acorta con el coseno de la latitud media
    // para que Guatemala no se vea estirada de lado.
    function proyectar(longitud, latitud, escalaX) {
        return [longitud * escalaX, -latitud];
    }

    function dibujarMapa(geojson) {
        var minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
        var escalaX = Math.cos(15.5 * Math.PI / 180);

        geojson.features.forEach(function (f) {
            f.geometry.coordinates.forEach(function (poligono) {
                poligono.forEach(function (anillo) {
                    anillo.forEach(function (c) {
                        var p = proyectar(c[0], c[1], escalaX);
                        minX = Math.min(minX, p[0]); maxX = Math.max(maxX, p[0]);
                        minY = Math.min(minY, p[1]); maxY = Math.max(maxY, p[1]);
                    });
                });
            });
        });

        var margen = 0.05;
        var svg = crear('svg', {
            viewBox: [minX - margen, minY - margen, maxX - minX + margen * 2, maxY - minY + margen * 2].join(' '),
            role: 'img',
            'aria-label': 'Mapa de Guatemala por departamentos'
        });

        var capaDepartamentos = crear('g');
        var capaEntregas = crear('g', { 'class': 'mapa-capa-entregas' });

        geojson.features.forEach(function (f) {
            var nombre = f.properties.nombre;
            var datosDepartamento = porNombre[nombre];
            if (!datosDepartamento) return;

            var ruta = f.geometry.coordinates.map(function (poligono) {
                return poligono.map(function (anillo) {
                    return 'M' + anillo.map(function (c) {
                        var p = proyectar(c[0], c[1], escalaX);
                        return p[0].toFixed(4) + ' ' + p[1].toFixed(4);
                    }).join('L') + 'Z';
                }).join('');
            }).join('');

            var camino = crear('path', {
                d: ruta,
                'class': 'mapa-departamento',
                tabindex: '0',
                role: 'button',
                'aria-label': nombre
            });
            camino.addEventListener('mousemove', function (e) { mostrarTooltip(e, nombre); });
            camino.addEventListener('mouseleave', ocultarTooltip);
            camino.addEventListener('focus', function () { resaltar(nombre, true); });
            camino.addEventListener('blur', function () { resaltar(nombre, false); });
            camino.addEventListener('click', function () { elegir(nombre); });
            camino.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); elegir(nombre); }
            });

            caminos[nombre] = camino;
            capaDepartamentos.appendChild(camino);

            var centro = centroDe(f, escalaX);
            var punto = crear('circle', { cx: centro[0], cy: centro[1], r: 0, 'class': 'mapa-punto-entrega' });
            punto.style.pointerEvents = 'none';
            puntos[nombre] = punto;
            capaEntregas.appendChild(punto);
        });

        svg.appendChild(capaDepartamentos);
        svg.appendChild(capaEntregas);
        contenedorMapa.innerHTML = '';
        contenedorMapa.appendChild(svg);

        pintar();
    }

    // Centro del polígono más grande del departamento (el territorio principal,
    // sin contar islas), para colocar ahí el punto de las entregas.
    function centroDe(feature, escalaX) {
        var mayor = null, mayorArea = -1;

        feature.geometry.coordinates.forEach(function (poligono) {
            var anillo = poligono[0];
            var xs = anillo.map(function (c) { return c[0]; });
            var ys = anillo.map(function (c) { return c[1]; });
            var ancho = Math.max.apply(null, xs) - Math.min.apply(null, xs);
            var alto = Math.max.apply(null, ys) - Math.min.apply(null, ys);
            if (ancho * alto > mayorArea) {
                mayorArea = ancho * alto;
                mayor = { x: (Math.max.apply(null, xs) + Math.min.apply(null, xs)) / 2, y: (Math.max.apply(null, ys) + Math.min.apply(null, ys)) / 2 };
            }
        });

        return proyectar(mayor.x, mayor.y, escalaX);
    }

    // ---------- Colores, leyenda y ranking ----------

    function pintar() {
        var indicador = INDICADORES[indicadorActual];

        datos.forEach(function (d) {
            if (caminos[d.nombre]) caminos[d.nombre].setAttribute('fill', colorDe(d));
        });

        var maximoEntregas = Math.max.apply(null, datos.map(function (d) { return d.entregasRealizadas; }).concat([1]));
        datos.forEach(function (d) {
            var punto = puntos[d.nombre];
            if (!punto) return;
            var radio = mostrarEntregas && d.entregasRealizadas > 0
                ? 0.035 + 0.075 * Math.sqrt(d.entregasRealizadas / maximoEntregas)
                : 0;
            punto.setAttribute('r', radio);
        });

        tituloMapa.textContent = indicador.titulo + ' por departamento, ' + indicador.fuente;
        dibujarLeyenda(indicador);
        dibujarRanking(indicador);
        marcarElegido();
    }

    function dibujarLeyenda(indicador) {
        var cortes = indicador.cortes;
        var textos = [
            'Menos de ' + cortes[0] + '%',
            cortes[0] + '% a ' + cortes[1] + '%',
            cortes[1] + '% a ' + cortes[2] + '%',
            cortes[2] + '% a ' + cortes[3] + '%',
            cortes[3] + '% o más'
        ];

        leyenda.innerHTML = '';
        COLORES.forEach(function (color, i) {
            var item = document.createElement('li');
            var muestra = document.createElement('span');
            muestra.className = 'leyenda-color';
            muestra.style.background = color;
            var texto = document.createElement('span');
            texto.className = 'leyenda-nombre';
            texto.textContent = textos[i];
            item.appendChild(muestra);
            item.appendChild(texto);
            leyenda.appendChild(item);
        });
    }

    function dibujarRanking(indicador) {
        var ordenados = datos.slice().sort(function (a, b) { return b[indicador.campo] - a[indicador.campo]; });
        var maximo = ordenados[0][indicador.campo];

        ranking.innerHTML = '';
        filasRanking = {};

        ordenados.forEach(function (d) {
            var fila = document.createElement('li');
            fila.className = 'ranking-fila';
            fila.tabIndex = 0;

            var nombre = document.createElement('span');
            nombre.className = 'ranking-nombre';
            nombre.textContent = d.nombre;

            var pista = document.createElement('span');
            pista.className = 'ranking-pista';
            var barra = document.createElement('span');
            barra.className = 'ranking-barra';
            barra.style.width = (d[indicador.campo] * 100 / maximo) + '%';
            barra.style.background = colorDe(d);
            pista.appendChild(barra);

            var valor = document.createElement('span');
            valor.className = 'ranking-valor';
            valor.textContent = porcentaje(d[indicador.campo]);

            fila.appendChild(nombre);
            fila.appendChild(pista);
            fila.appendChild(valor);

            fila.addEventListener('click', function () { elegir(d.nombre); });
            fila.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); elegir(d.nombre); }
            });
            fila.addEventListener('mouseenter', function () { resaltar(d.nombre, true); });
            fila.addEventListener('mouseleave', function () { resaltar(d.nombre, false); });

            filasRanking[d.nombre] = fila;
            ranking.appendChild(fila);
        });
    }

    // ---------- Interacción ----------

    function resaltar(nombre, activo) {
        if (caminos[nombre]) caminos[nombre].classList.toggle('resaltado', activo);
    }

    function elegir(nombre) {
        departamentoElegido = departamentoElegido === nombre ? null : nombre;
        marcarElegido();
        dibujarDetalle();
    }

    function marcarElegido() {
        Object.keys(caminos).forEach(function (n) {
            caminos[n].classList.toggle('elegido', n === departamentoElegido);
        });
        Object.keys(filasRanking).forEach(function (n) {
            filasRanking[n].classList.toggle('elegida', n === departamentoElegido);
        });
    }

    function mostrarTooltip(evento, nombre) {
        var d = porNombre[nombre];
        var caja = contenedorMapa.getBoundingClientRect();

        tooltip.innerHTML = '';
        var titulo = document.createElement('strong');
        titulo.textContent = nombre;
        tooltip.appendChild(titulo);

        [['Pobreza general', porcentaje(d.pobrezaTotal2023)], ['Pobreza extrema', porcentaje(d.pobrezaExtrema2023)], ['Multidimensional', porcentaje(d.pobrezaMultidimensional2023)]].forEach(function (par) {
            var linea = document.createElement('span');
            linea.textContent = par[0] + ': ' + par[1];
            tooltip.appendChild(linea);
        });

        tooltip.hidden = false;
        var izquierda = evento.clientX - caja.left + 14;
        var arriba = evento.clientY - caja.top + 14;
        if (izquierda + tooltip.offsetWidth > caja.width) izquierda -= tooltip.offsetWidth + 28;
        tooltip.style.left = izquierda + 'px';
        tooltip.style.top = arriba + 'px';
    }

    function ocultarTooltip() {
        tooltip.hidden = true;
    }

    function filaDetalle(etiqueta, valor) {
        var fila = document.createElement('div');
        fila.className = 'detalle-fila';
        var e = document.createElement('span');
        e.textContent = etiqueta;
        var v = document.createElement('strong');
        v.textContent = valor;
        fila.appendChild(e);
        fila.appendChild(v);
        return fila;
    }

    function cambioDe(actual, anterior) {
        var diferencia = actual - anterior;
        var signo = diferencia > 0 ? '+' : '';
        return signo + diferencia.toLocaleString('es-GT', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + ' puntos desde 2014';
    }

    function dibujarDetalle() {
        panelDetalle.innerHTML = '';

        if (!departamentoElegido) {
            var ayuda = document.createElement('p');
            ayuda.className = 'detalle-ayuda';
            ayuda.textContent = 'Toca un departamento del mapa o de la lista para ver su detalle.';
            panelDetalle.appendChild(ayuda);
            return;
        }

        var d = porNombre[departamentoElegido];
        var titulo = document.createElement('h4');
        titulo.textContent = d.nombre;
        panelDetalle.appendChild(titulo);

        panelDetalle.appendChild(filaDetalle('Pobreza general 2023', porcentaje(d.pobrezaTotal2023)));
        panelDetalle.appendChild(filaDetalle('Cambio', cambioDe(d.pobrezaTotal2023, d.pobrezaTotal2014)));
        panelDetalle.appendChild(filaDetalle('Pobreza extrema 2023', porcentaje(d.pobrezaExtrema2023)));
        panelDetalle.appendChild(filaDetalle('Cambio', cambioDe(d.pobrezaExtrema2023, d.pobrezaExtrema2014)));
        panelDetalle.appendChild(filaDetalle('Pobreza multidimensional', porcentaje(d.pobrezaMultidimensional2023)));

        var subtitulo = document.createElement('h5');
        subtitulo.textContent = 'Lo que hace la campaña aquí';
        panelDetalle.appendChild(subtitulo);

        panelDetalle.appendChild(filaDetalle('Entregas realizadas', d.entregasRealizadas));
        panelDetalle.appendChild(filaDetalle('Familias atendidas', d.familiasAtendidas.toLocaleString('es-GT')));
        panelDetalle.appendChild(filaDetalle('Envíos por hacer', d.enviosPendientes));
    }

    // ---------- Controles ----------

    document.querySelectorAll('[data-indicador]').forEach(function (boton) {
        boton.addEventListener('click', function () {
            indicadorActual = boton.dataset.indicador;
            document.querySelectorAll('[data-indicador]').forEach(function (b) {
                var activo = b === boton;
                b.classList.toggle('activo', activo);
                b.setAttribute('aria-pressed', activo);
            });
            pintar();
        });
    });

    document.getElementById('verEntregas').addEventListener('change', function (e) {
        mostrarEntregas = e.target.checked;
        pintar();
    });

    dibujarDetalle();

    fetch(contenedorMapa.dataset.origen)
        .then(function (respuesta) { return respuesta.json(); })
        .then(dibujarMapa)
        .catch(function () {
            contenedorMapa.textContent = 'No se pudo cargar el mapa. Recarga la página para intentarlo de nuevo.';
        });
})();
