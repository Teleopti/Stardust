define(
    [
        'noext!../../../../signalr/hubs'
    ], function(
        signalRHubs
    ) {

        return {
            start: function() {
                $.connection.hub.url = 'signalr';
                return $.connection.hub.start();
            }
        };
        
    });
