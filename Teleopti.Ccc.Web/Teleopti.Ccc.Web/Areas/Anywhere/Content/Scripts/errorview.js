define(
    [
		'text!templates/error.html'
    ], function (
        errorTemplate
    ) {

        function _displayError(incoming) {
            
            var display = {
                title: "Error!",
                message: "No message!"
            };
            
            if (typeof incoming != "string") {
                display.title = incoming.title || display.title;
                if (typeof incoming.message != "string") {
                    display.message = JSON.stringify(incoming.message);
                } else {
                    display.message = incoming.message || display.message;
                }
            } else {
                display.message = incoming;
            }

            var placeHolder = $('#error-placeholder')
                .html(errorTemplate)
                ;
            placeHolder
                .find('strong')
                .text(display.title);
            placeHolder
                .find('span')
                .text(display.message);
        }

        function _removeError() {
            $('#error-placeholder').html("");
        }

        return {
        	display: function (message) {
        		_displayError(message);
        	},
        	remove: _removeError
        };
        
    });
