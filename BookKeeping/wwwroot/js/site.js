// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Anti-duplicate-submission guard for all forms
$(document).ready(function () {
    $('form').on('submit', function () {
        var $form = $(this);
        var $submitButton = $form.find('button[type="submit"]');
        var confirmMessage = $form.data('confirm-message');

        if (confirmMessage && !window.confirm(confirmMessage)) {
            return false;
        }
        
        // Don't disable if already disabled
        if ($submitButton.prop('disabled')) {
            return false;
        }
        
        // Disable submit button and change text
        $submitButton.prop('disabled', true);
        var originalText = $submitButton.text();
        $submitButton.data('original-text', originalText);
        $submitButton.text('處理中...');
        
        // Re-enable on validation failure (after a short delay to allow validation to run)
        setTimeout(function () {
            if ($form.find('.field-validation-error').length > 0 || 
                $form.find('.validation-summary-errors').length > 0) {
                $submitButton.prop('disabled', false);
                $submitButton.text($submitButton.data('original-text'));
            }
        }, 100);
    });
});

window.bookKeeping = window.bookKeeping || {};

window.bookKeeping.showToast = function (message, toastType) {
    if (!message) {
        return;
    }

    var toastClass = toastType === 'warning'
        ? 'bg-warning text-dark'
        : toastType === 'error'
            ? 'bg-danger text-white'
            : 'bg-success text-white';
    var closeButtonClass = toastType === 'warning' ? 'btn-close' : 'btn-close btn-close-white';
    var toastContainer = getOrCreateToastContainer();
    var toastElement = document.createElement('div');
    toastElement.className = 'toast align-items-center ' + toastClass + ' border-0';
    toastElement.setAttribute('role', 'alert');
    toastElement.setAttribute('aria-live', 'assertive');
    toastElement.setAttribute('aria-atomic', 'true');
    toastElement.innerHTML =
        '<div class="d-flex">' +
        '<div class="toast-body">' + message + '</div>' +
        '<button type="button" class="' + closeButtonClass + ' me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
        '</div>';

    toastContainer.appendChild(toastElement);
    var toast = new bootstrap.Toast(toastElement, {
        autohide: false
    });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
};

window.bookKeeping.checkBudgetStatusAndToast = async function (checkStatusUrl, categoryId) {
    if (!checkStatusUrl || !categoryId) {
        return;
    }

    var queryToken = checkStatusUrl.indexOf('?') >= 0 ? '&' : '?';
    var requestUrl = checkStatusUrl + queryToken + 'categoryId=' + encodeURIComponent(categoryId);

    try {
        var response = await fetch(requestUrl, {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            return;
        }

        var payload = await response.json();
        if (payload.status === 'warning') {
            window.bookKeeping.showToast(payload.message, 'warning');
        } else if (payload.status === 'exceeded') {
            window.bookKeeping.showToast(payload.message, 'error');
        }
    } catch (error) {
        console.error('Budget status check failed.', error);
    }
};

function getOrCreateToastContainer() {
    var existingContainer = document.querySelector('.bookkeep-toast-container');
    if (existingContainer) {
        return existingContainer;
    }

    var container = document.createElement('div');
    container.className = 'toast-container position-fixed top-0 end-0 p-3 bookkeep-toast-container';
    document.body.appendChild(container);
    return container;
}
