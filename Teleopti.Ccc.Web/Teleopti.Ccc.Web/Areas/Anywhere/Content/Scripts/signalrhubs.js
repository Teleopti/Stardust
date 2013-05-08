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

        return {
            start: function() {
                $.connection.hub.url = 'signalr';
                $.connection.hub.error(errorview.display);
                return $.connection.hub.start();
            }
        };
        
    });
