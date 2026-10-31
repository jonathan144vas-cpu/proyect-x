// Modo claro / oscuro: se guarda la elección y se avisa a las gráficas y al mapa.
(function () {
    var raiz = document.documentElement;
    var boton = document.getElementById('botonTema');
    var texto = document.getElementById('textoTema');

    function actual() {
        return raiz.getAttribute('data-tema') === 'claro' ? 'claro' : 'oscuro';
    }

    function etiquetar() {
        var claro = actual() === 'claro';
        if (texto) texto.textContent = claro ? 'Modo claro' : 'Modo oscuro';
        if (boton) boton.setAttribute('aria-pressed', claro ? 'true' : 'false');
    }

    window.temaActual = actual;
    etiquetar();

    if (!boton) return;

    boton.addEventListener('click', function () {
        var nuevo = actual() === 'claro' ? 'oscuro' : 'claro';
        raiz.classList.add('tema-animado');
        raiz.setAttribute('data-tema', nuevo);
        try { localStorage.setItem('tema', nuevo); } catch (e) { }
        var meta = document.getElementById('metaColorScheme');
        if (meta) meta.setAttribute('content', nuevo === 'claro' ? 'only light' : 'dark');
        etiquetar();
        document.dispatchEvent(new CustomEvent('tema-cambiado', { detail: nuevo }));
    });
})();

// Abrir y cerrar el menú lateral en pantallas de teléfono.
(function () {
    var boton = document.getElementById('botonMenu');
    var menu = document.getElementById('menuLateral');
    var fondo = document.getElementById('fondoMenu');

    if (!boton || !menu || !fondo) {
        return;
    }

    function alternarMenu() {
        menu.classList.toggle('abierto');
        fondo.classList.toggle('visible');
    }

    function cerrarMenu() {
        menu.classList.remove('abierto');
        fondo.classList.remove('visible');
    }

    boton.addEventListener('click', alternarMenu);
    fondo.addEventListener('click', cerrarMenu);

    document.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape') {
            cerrarMenu();
        }
    });
})();

// Cuenta el porcentaje de 0 hasta su valor, en sincronía con la barra que se llena.
(function () {
    var contadores = document.querySelectorAll('[data-contador]');
    if (!contadores.length) return;

    var sinMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    contadores.forEach(function (elemento) {
        var destino = parseInt(elemento.dataset.contador, 10) || 0;

        if (sinMovimiento || destino === 0) {
            elemento.textContent = destino;
            return;
        }

        var duracion = 1300;
        var inicio = null;

        function paso(ahora) {
            if (inicio === null) inicio = ahora;
            var avance = Math.min((ahora - inicio) / duracion, 1);
            // Misma curva que usa la barra, para que lleguen juntos.
            var suave = 1 - Math.pow(1 - avance, 3);
            elemento.textContent = Math.round(destino * suave);
            if (avance < 1) requestAnimationFrame(paso);
        }

        requestAnimationFrame(paso);
    });
})();

// Barra delgada arriba mientras carga la siguiente pantalla.
(function () {
    var barra = document.createElement('div');
    barra.id = 'barraCarga';
    document.body.appendChild(barra);

    var temporizador = null;

    function arrancar() {
        barra.classList.add('activa');
        barra.style.width = '25%';
        clearTimeout(temporizador);
        temporizador = setTimeout(function () { barra.style.width = '75%'; }, 220);
    }

    // Enlaces internos que sí navegan (se ignoran anclas, descargas y otras pestañas)
    document.addEventListener('click', function (evento) {
        var enlace = evento.target.closest('a');
        if (!enlace) return;
        if (enlace.target === '_blank' || enlace.hasAttribute('download')) return;

        var href = enlace.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;
        if (enlace.origin && enlace.origin !== window.location.origin) return;

        arrancar();
    });

    document.addEventListener('submit', arrancar);

    // Si se regresa con el botón "atrás", la página vuelve del caché: hay que limpiarla.
    window.addEventListener('pageshow', function () {
        clearTimeout(temporizador);
        barra.classList.remove('activa');
        barra.style.width = '0';
    });
})();

// Si la fila recién agregada queda fuera de la pantalla, se lleva a la vista.
(function () {
    var fila = document.querySelector('tr.fila-nueva');
    if (!fila) return;
    setTimeout(function () { fila.scrollIntoView({ block: 'center', behavior: 'auto' }); }, 150);
})();
