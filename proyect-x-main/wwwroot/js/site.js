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
