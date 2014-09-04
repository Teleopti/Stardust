define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var startPromise;

	var agentAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!agentAdherenceSubscription)
			return;
		startPromise.done(function () {
			messagebroker.unsubscribe(agentAdherenceSubscription);
			agentAdherenceSubscription = null;
		});
	};

	return {
		start: function () {
			startPromise = messagebroker.start();
			return startPromise;
		},

		subscribeAdherence: function (callback,businessUnitId, teamId, subscriptionDone, multipleSubscription) {
			if (!multipleSubscription) {
				unsubscribeAdherence();
			}
			startPromise.done(function () {
				agentAdherenceSubscription = messagebroker.subscribe({
					domainType: 'AgentsAdherenceMessage',
					businessUnitId: businessUnitId,
					domainReferenceId: teamId,
					callback: callback
				});
				agentAdherenceSubscription.promise.done(function() {
					subscriptionDone();
				});
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
