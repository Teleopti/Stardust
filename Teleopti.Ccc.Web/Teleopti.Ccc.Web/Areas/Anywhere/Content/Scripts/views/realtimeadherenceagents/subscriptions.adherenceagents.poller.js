define([
	'jquery',
	'ajax'
], function (
	$,
	ajax
	) {
	var agentAdherencePollers = [];
	var unsubscribeAdherence = function () {
		if (!agentAdherencePollers)
			return;
		for (var i = 0; i < agentAdherencePollers.length; i++) {
			clearInterval(agentAdherencePollers[i]);
		}

		clearPollers();
	};

	var clearPollers = function() {
		while (agentAdherencePollers.length > 0) {
			agentAdherencePollers.pop();
		};
	}

	var mapAsNotification = function (data) {
		return {
			BinaryData: JSON.stringify({ AgentStates: data })
		}
	}

	return {
		start: function () {
			return $.Deferred().resolve();
		},

		subscribeAdherence: function (callback, businessUnitId, teamId, subscriptionDone) {
			var agentPoller = setInterval(function () {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					url: "Agents/GetStates?teamId=" + teamId,
					success: function (data) {
						callback(mapAsNotification(data));
					}
				});
			}, 2000);

			agentAdherencePollers.push(agentPoller);
			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
