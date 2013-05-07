define(
    [
		'text!templates/error.html'
    ], function (
        errorTemplate
    ) {

        function _initAjaxErrorCallback() {
            $(document).ajaxError(function (e, jqxhr, settings, exception) {
                _displayError(exception);
            });
        }

        function _displayError(message) {
            $('#error-placeholder')
                .html(errorTemplate)
                .find('span')
                .text(message);
        }

        function _removeError() {
            $('#error-placeholder').html("");
        }

        _initAjaxErrorCallback();
        
        return {
            display: function(message) {
                _displayError(message);
            },
            remove: _removeError
        };
        
    });
