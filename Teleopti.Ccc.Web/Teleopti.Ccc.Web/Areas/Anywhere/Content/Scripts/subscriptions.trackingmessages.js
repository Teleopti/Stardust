define([
	'jquery',
	'messagebroker'
], function(
	$,
	messagebroker
) {

	var trackingMessageSubscription = null;

	var unsubscribeTrackingMessage = function() {
		if (!trackingMessageSubscription)
			return;
		messagebroker.unsubscribe(trackingMessageSubscription);
		trackingMessageSubscription = null;
	};

	return {
		subscribeTrackingMessage: function(buId, personId, callback, subscriptionDone) {
			unsubscribeTrackingMessage();
			messagebroker.started.done(function () {
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