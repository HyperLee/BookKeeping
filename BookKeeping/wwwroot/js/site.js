// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Anti-duplicate-submission guard for all forms
$(document).ready(function () {
    $('form').on('submit', function () {
        var $form = $(this);
        var $submitButton = $form.find('button[type="submit"]');
        
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

