define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var startPromise;

	var teamAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!teamAdherenceSubscription)
			return;
		startPromise.done(function () {
			messagebroker.unsubscribe(teamAdherenceSubscription);
			teamAdherenceSubscription = null;
		});
	};

	return {
		start: function () {
			startPromise = messagebroker.start();
			return startPromise;
		},

		subscribeAdherence: function (callback, businessUnitId, siteId, subscriptionDone) {
			unsubscribeAdherence();
			startPromise.done(function () {
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
