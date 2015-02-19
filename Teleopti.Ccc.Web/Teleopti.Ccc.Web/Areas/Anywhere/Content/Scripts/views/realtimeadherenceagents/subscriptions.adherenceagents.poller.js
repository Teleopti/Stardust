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

	var load = function (callback, businessUnitId, teamId) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "Agents/GetStates?teamId=" + teamId,
			success: function (data) {
				callback(mapAsNotification(data));
			}
		});
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, teamId, subscriptionDone) {

			var poll = function() {
				load(callback, businessUnitId, teamId);
			}

			setTimeout(poll, 100);
			var poller = setInterval(poll, 2000);

			agentAdherencePollers.push(poller);
			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
