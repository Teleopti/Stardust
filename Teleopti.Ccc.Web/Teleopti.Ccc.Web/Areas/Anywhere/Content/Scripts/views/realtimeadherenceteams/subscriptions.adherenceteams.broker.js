define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var teamAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!teamAdherenceSubscription)
			return;
		messagebroker.unsubscribe(teamAdherenceSubscription);
		teamAdherenceSubscription = null;
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, siteId, subscriptionDone) {
			unsubscribeAdherence();
			messagebroker.started.done(function () {
				teamAdherenceSubscription = messagebroker.subscribe({
					domainType: 'TeamAdherenceMessage',
					businessUnitId: businessUnitId,
					domainReferenceId: siteId,
					callback: callback
				});
				teamAdherenceSubscription.promise.done(function () {
					subscriptionDone();
				});
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
