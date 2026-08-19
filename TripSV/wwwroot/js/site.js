document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-dismissible').forEach(function (alerta) {
        window.setTimeout(function () {
            alerta.classList.remove('show');
        }, 6000);
    });
});
