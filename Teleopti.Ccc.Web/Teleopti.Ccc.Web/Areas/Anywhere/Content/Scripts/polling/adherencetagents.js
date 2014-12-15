define([
	'jquery',
	'ajax'
], function (
	$,
	ajax
	) {
	var siteAdherencePoller = null;
	var unsubscribeAdherence = function () {
		if (!siteAdherencePoller)
			return;
		clearInterval(siteAdherencePoller);
		siteAdherencePoller = null;
	};

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
			siteAdherencePoller = setInterval(function () {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					url: "Agents/GetStates?teamId=" + teamId,
					success: function (data) {
						callback(mapAsNotification(data));
					}
				});
			}, 1000);

			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
