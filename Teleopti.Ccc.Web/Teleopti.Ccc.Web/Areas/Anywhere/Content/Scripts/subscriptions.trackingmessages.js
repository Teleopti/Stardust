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

		subscribeTrackingMessage: function(buId, personId, callback, subscriptionDone) {
			unsubscribeTrackingMessage();
			startPromise.done(function() {
				trackingMessageSubscription = messagebroker.subscribe({
					businessUnitId: buId,
					domainType: 'TrackingMessage',
					domainReferenceId: personId,
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