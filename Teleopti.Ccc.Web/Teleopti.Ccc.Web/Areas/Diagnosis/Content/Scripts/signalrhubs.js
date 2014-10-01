define(
    [
        'signalrrr!r'
    ], function (
        signalRHubs
    ) {

        return {
        	start: function (options) {
        		if (options && options.url) {
        			$.connection.hub.url = options.url;
		        } else {
        			$.connection.hub.url = 'signalr';
		        }
                return $.connection.hub.start();
            }
        };
        
    });
