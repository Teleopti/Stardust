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
	var idsToUrl = function (idtype, ids) {
		var idsUrl = "";
		ids.forEach(function (id) {
			idsUrl += idtype + "=" + id + "&";
		});
		idsUrl = idsUrl.substring(0, idsUrl.length - 1);
		return idsUrl;
	};

	var load = function (callback, businessUnitId, statesUrl) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: statesUrl,
			success: function (data) {
				callback(mapAsNotification(data));
			}
		});
	};

	var loadForTeam = function(callback, businessUnitId, teamId) {
		load(callback, businessUnitId, "Agents/GetStates?teamId=" + teamId);
	};

	var loadForSites = function(callback, businessUnitId, siteIds) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "Agents/GetStatesForSites?" + idsToUrl("siteIds", siteIds),
			success: function(data) {
				callback(mapAsNotification(data));
			}
		});
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, teamId, subscriptionDone) {

			var poll = function() {
				loadForTeam(callback, businessUnitId, teamId);
			}

			setTimeout(poll, 100);
			var poller = setInterval(poll, 2000);

			agentAdherencePollers.push(poller);
			subscriptionDone();
		},

		subscribeForSitesAdherence: function (callback, businessUnitId, siteIds, subscriptionDone) {

			var poll = function () {
				loadForSites(callback, businessUnitId, siteIds);
			}

			setTimeout(poll, 100);
			var poller = setInterval(poll, 2000);

			agentAdherencePollers.push(poller);
			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
