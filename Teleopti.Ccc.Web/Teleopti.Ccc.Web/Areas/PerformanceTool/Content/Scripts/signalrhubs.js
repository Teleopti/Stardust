define(
    [
        'signalrrr!r'
    ], function (
        signalRHubs
    ) {

        return {
            start: function() {
                $.connection.hub.url = 'signalr';
                return $.connection.hub.start();
            }
        };
        
    });
