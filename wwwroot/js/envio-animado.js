window.animarEnvio = function (boton, alTerminar) {
    if (boton.classList.contains("is-loading")) {
        return;
    }

    boton.classList.remove("is-success");
    boton.classList.add("is-loading");
    boton.disabled = true;
    boton.setAttribute("aria-busy", "true");

    // Dar tiempo para que el camión complete su recorrido.
    window.setTimeout(function () {
        boton.classList.remove("is-loading");
        boton.classList.add("is-success");
        boton.removeAttribute("aria-busy");

        const mensaje = boton.querySelector(".envio-btn__exito");

        if (mensaje) {
            mensaje.setAttribute("aria-hidden", "false");
        }

        window.setTimeout(function () {
            if (typeof alTerminar === "function") {
                alTerminar();
            }
        }, 500);

    }, 2500);
};
