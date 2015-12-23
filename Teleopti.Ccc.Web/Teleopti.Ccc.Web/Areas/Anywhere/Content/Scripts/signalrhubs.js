define(
    [
		'text!templates/error.html',
        'errorview',
        'signalrhubs.started',

		'messagebroker',
		'subscriptions.groupschedule',
		'subscriptions.personschedule',
		//'views/realtimeadherenceteams/subscriptions.adherenceteams',
		//'views/realtimeadherencesites/subscriptions.adherencesites',
		//'views/realtimeadherenceagents/subscriptions.adherenceagents',
		//'subscriptions.trackingmessages'

    ], function (
        errorTemplate,
        errorview,
		started
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
			    var promise = $.connection.hub.start();
			    promise.done(function () {
				    started.resolve();
			    });
			    return promise;
		    }
    	};

    });
