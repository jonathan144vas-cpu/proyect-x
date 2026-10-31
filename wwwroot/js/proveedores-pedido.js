// Lista de proveedores del formulario de pedidos: solo muestra los de la categoría elegida
// y termina con la opción "Agregar proveedor", que abre un cuadro para registrar uno nuevo.
(function () {
    var listaCategoria = document.getElementById('Categoria');
    var listaProveedor = document.getElementById('ProveedorId');
    var dialogo = document.getElementById('dialogoProveedor');
    if (!listaCategoria || !listaProveedor || !dialogo) return;

    var formulario = document.getElementById('formularioProveedor');
    var categoriaDelDialogo = document.getElementById('proveedorCategoria');
    var errorGeneral = document.getElementById('errorProveedor');
    var proveedores = window.proveedoresDisponibles || [];

    var AGREGAR = 'agregar';
    var elegido = listaProveedor.dataset.elegido || '';

    function agregarOpcion(valor, texto) {
        var opcion = document.createElement('option');
        opcion.value = valor;
        opcion.textContent = texto;
        listaProveedor.appendChild(opcion);
    }

    function llenarLista(idElegido) {
        listaProveedor.innerHTML = '';

        if (!listaCategoria.value) {
            agregarOpcion('', 'Primero elige una categoría');
            listaProveedor.disabled = true;
            return;
        }

        listaProveedor.disabled = false;
        agregarOpcion('', 'Sin proveedor');

        proveedores
            .filter(function (p) { return p.categoria === listaCategoria.value; })
            .forEach(function (p) { agregarOpcion(p.id, p.nombre); });

        agregarOpcion(AGREGAR, '+ Agregar proveedor');

        listaProveedor.value = idElegido ? String(idElegido) : '';
        elegido = listaProveedor.value;
    }

    function limpiarErrores() {
        errorGeneral.hidden = true;
        errorGeneral.textContent = '';
        formulario.querySelectorAll('[data-valmsg-for]').forEach(function (aviso) {
            aviso.textContent = '';
        });
    }

    function abrirDialogo() {
        formulario.reset();
        limpiarErrores();
        categoriaDelDialogo.value = listaCategoria.value;
        dialogo.showModal();
    }

    function mostrarErrores(errores) {
        var sinCampo = [];

        Object.keys(errores).forEach(function (campo) {
            var aviso = formulario.querySelector('[data-valmsg-for="' + campo + '"]');
            if (aviso) {
                aviso.textContent = errores[campo][0];
            } else {
                sinCampo = sinCampo.concat(errores[campo]);
            }
        });

        if (sinCampo.length) {
            errorGeneral.textContent = sinCampo.join(' ');
            errorGeneral.hidden = false;
        }
    }

    listaCategoria.addEventListener('change', function () { llenarLista(null); });

    listaProveedor.addEventListener('change', function () {
        if (listaProveedor.value === AGREGAR) {
            listaProveedor.value = elegido;
            abrirDialogo();
            return;
        }
        elegido = listaProveedor.value;
    });

    document.getElementById('cancelarProveedor').addEventListener('click', function () { dialogo.close(); });

    formulario.addEventListener('submit', function (evento) {
        evento.preventDefault();
        limpiarErrores();

        // Si jQuery Validation está cargado, se usa para avisar antes de enviar.
        if (window.jQuery && window.jQuery.fn.valid && !window.jQuery(formulario).valid()) return;

        var boton = formulario.querySelector('button[type="submit"]');
        boton.disabled = true;

        fetch(formulario.action, { method: 'POST', body: new FormData(formulario) })
            .then(function (respuesta) {
                return respuesta.json().then(function (datos) { return { ok: respuesta.ok, datos: datos }; });
            })
            .then(function (resultado) {
                if (!resultado.ok) {
                    mostrarErrores(resultado.datos.errores || {});
                    return;
                }

                var nuevo = resultado.datos;
                proveedores.push(nuevo);
                dialogo.close();

                // Si el proveedor es de otra área, el pedido queda como estaba.
                if (nuevo.categoria === listaCategoria.value) {
                    llenarLista(nuevo.id);
                }

                if (window.Swal) {
                    Swal.fire({
                        toast: true, position: 'top-end', icon: 'success',
                        title: 'Proveedor agregado: ' + nuevo.nombre,
                        background: '#1e293b', color: '#f8fafc',
                        showConfirmButton: false, timer: 3000, timerProgressBar: true
                    });
                }
            })
            .catch(function () {
                errorGeneral.textContent = 'No se pudo guardar el proveedor. Revisa tu conexión e inténtalo de nuevo.';
                errorGeneral.hidden = false;
            })
            .then(function () { boton.disabled = false; });
    });

    llenarLista(elegido);
})();
