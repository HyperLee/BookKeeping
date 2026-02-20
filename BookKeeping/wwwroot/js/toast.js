// Toast initialization
document.addEventListener('DOMContentLoaded', function () {
    const toastElements = document.querySelectorAll('.toast');
    toastElements.forEach(function (toastEl) {
        const autoDismiss = toastEl.getAttribute('data-auto-dismiss') === 'true';
        const toast = new bootstrap.Toast(toastEl, {
            autohide: autoDismiss,
            delay: 3000
        });
        toast.show();
    });
});
