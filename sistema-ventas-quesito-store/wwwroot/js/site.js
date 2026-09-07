// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ── Sistema de mensajes internos (reemplaza a los alert() del navegador) ──
// Uso: qsToast('Mensaje', 'success' | 'error' | 'info')
(function () {
    function contenedor() {
        var c = document.getElementById('qsToastContainer');
        if (!c) {
            c = document.createElement('div');
            c.id = 'qsToastContainer';
            c.className = 'qs-toast-container';
            document.body.appendChild(c);
        }
        return c;
    }

    window.qsToast = function (mensaje, tipo) {
        tipo = tipo || 'info';
        var iconos = { success: 'bi-check-circle-fill', error: 'bi-x-circle-fill', info: 'bi-info-circle-fill' };
        var icono = iconos[tipo] || iconos.info;

        var toast = document.createElement('div');
        toast.className = 'qs-toast qs-toast-' + tipo;
        toast.innerHTML = '<i class="bi ' + icono + ' me-2"></i><span></span>';
        toast.querySelector('span').textContent = mensaje;

        contenedor().appendChild(toast);
        requestAnimationFrame(function () { toast.classList.add('qs-toast-show'); });

        setTimeout(function () {
            toast.classList.remove('qs-toast-show');
            setTimeout(function () { toast.remove(); }, 250);
        }, 3800);
    };
})();

