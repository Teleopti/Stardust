define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var agentAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!agentAdherenceSubscription)
			return;
		messagebroker.unsubscribe(agentAdherenceSubscription);
		agentAdherenceSubscription = null;
	};

	return {
		subscribeAdherence: function (callback,businessUnitId, teamId, subscriptionDone, multipleSubscription) {
			if (!multipleSubscription) {
				unsubscribeAdherence();
			}
			messagebroker.started.done(function () {
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
