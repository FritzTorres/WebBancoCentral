// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function() {
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);

    // Confirmación para acciones destructivas
    $('.btn-danger').on('click', function(e) {
        if (!confirm('¿Está seguro de que desea realizar esta acción?')) {
            e.preventDefault();
        }
    });

    // Formatear campos de moneda
    $('.currency-input').on('input', function() {
        let value = $(this).val();
        value = value.replace(/[^\d.]/g, '');
        value = parseFloat(value);
        if (!isNaN(value)) {
            $(this).val(value.toFixed(2));
        }
    });

    // Validación de formularios
    $('form').on('submit', function() {
        $(this).find('button[type="submit"]').prop('disabled', true);
    });

    // Tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
});

// Función para mostrar notificaciones
function showNotification(message, type = 'success') {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    const container = $('.container').first();
    container.prepend(alertHtml);
    
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}

// Función para confirmar acciones
function confirmAction(message) {
    return confirm(message || '¿Está seguro de que desea realizar esta acción?');
}

// Función para formatear fechas
function formatDate(date) {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('es-DO', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Función para formatear moneda
function formatCurrency(amount, currency = 'DOP') {
    return new Intl.NumberFormat('es-DO', {
        style: 'currency',
        currency: currency
    }).format(amount);
}