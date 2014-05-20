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

		subscribeAdherence: function (callback, teamId, subscriptionDone) {
			unsubscribeAdherence();
			startPromise.done(function () {
				agentAdherenceSubscription = messagebroker.subscribe({
					domainType: 'AgentsAdherenceMessage',
					domainReferenceId: teamId,
					callback: callback
				});
				subscriptionDone();
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
