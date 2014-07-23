define(
    [
        'signalrrr!r',
		'text!templates/error.html',
        'errorview'
    ], function (
        signalRHubs,
        errorTemplate,
        errorview
    ) {
    	var hubErrorHandler = function (message) {
    		if (message.status === 403)
    			window.location.href = "";
    		else {
    			errorview.display(message);
    		}
    	};
    	return {
    		start: function () {
    			$.connection.hub.url = 'signalr';
    			$.connection.hub.error(hubErrorHandler);
    			return $.connection.hub.start();
    		}
    	};

    });
