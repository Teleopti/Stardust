define(
    [
		'text!templates/error.html'
    ], function (
        errorTemplate
    ) {

        function _initAjaxErrorCallback() {
            $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
                _displayError(thrownError);
            });
        }

        function _displayError(message) {
            message = message || "No message!";
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
