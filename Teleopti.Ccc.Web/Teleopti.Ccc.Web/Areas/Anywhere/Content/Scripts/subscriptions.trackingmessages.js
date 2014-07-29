define([
	'jquery',
	'messagebroker'
], function(
	$,
	messagebroker
) {

	var startPromise;

	var trackingMessageSubscription = null;

	var unsubscribeTrackingMessage = function() {
		if (!trackingMessageSubscription)
			return;
		startPromise.done(function() {
			messagebroker.unsubscribe(trackingMessageSubscription);
			trackingMessageSubscription = null;
		});
	};

	return {
		start: function() {
			startPromise = messagebroker.start();
			return startPromise;
		},

		subscribeTrackingMessage: function(callback, subscriptionDone) {
			unsubscribeTrackingMessage();
			startPromise.done(function() {
				trackingMessageSubscription = messagebroker.subscribe({
					domainType: 'TrackingMessage',
					callback: callback
				});
				trackingMessageSubscription.promise.done(function() {
					subscriptionDone();
				});
			});
		},

		unsubscribeTrackingMessage: unsubscribeTrackingMessage
	};

});