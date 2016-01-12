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
		load(callback, businessUnitId, "api/Agents/GetStates?teamId=" + teamId);
	};

	var loadForSites = function(callback, businessUnitId, siteIds) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "api/Agents/GetStatesForSites?" + idsToUrl("ids", siteIds),
			success: function(data) {
				callback(mapAsNotification(data));
			}
		});
	};

	var loadForTeams = function(callback, businessUnitId, teamIds) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "api/Agents/GetStatesForTeams?" + idsToUrl("ids", teamIds),
			success: function(data) {
				callback(mapAsNotification(data));
			}
		});
	};

	var configPoller = function(action) {
		setTimeout(action, 100);
		var poller = setInterval(action, 5000);

		agentAdherencePollers.push(poller);
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, teamId, subscriptionDone) {
			var poll = function() {
				loadForTeam(callback, businessUnitId, teamId);
			}
			configPoller(poll);
			subscriptionDone();
		},

		subscribeForSitesAdherence: function (callback, businessUnitId, siteIds, subscriptionDone) {
			var poll = function () {
				loadForSites(callback, businessUnitId, siteIds);
			}

			configPoller(poll);
			subscriptionDone();
		},

		subscribeForTeamsAdherence: function (callback, businessUnitId, teamIds, subscriptionDone) {
			var poll = function () {
				loadForTeams(callback, businessUnitId, teamIds);
			}

			configPoller(poll);
			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
