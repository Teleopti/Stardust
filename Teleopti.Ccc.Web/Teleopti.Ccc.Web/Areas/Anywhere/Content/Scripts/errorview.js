define(
    [
		'text!templates/error.html'
    ], function (
        errorTemplate
    ) {

        function _displayError(message) {
            message = message || "No message!";
            if (typeof message != "string") {
                message = JSON.stringify(message);
            }
            $('#error-placeholder')
                .html(errorTemplate)
                .find('span')
                .text(message);
        }

        function _removeError() {
            $('#error-placeholder').html("");
        }

        return {
            display: function(message) {
                _displayError(message);
            },
            remove: _removeError
        };
        
    });
