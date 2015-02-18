define([
	'jquery',
	'ajax'
], function (
	$,
	ajax
	) {
	var teamAdherencePoller = null;
	var unsubscribeAdherence = function () {
		if (!teamAdherencePoller)
			return;
		clearInterval(teamAdherencePoller);
		teamAdherencePoller = null;
	};

	var mapAsNotification = function (data) {
		return {
			DomainId: data.Id,
			BinaryData: JSON.stringify(data)
		}
	}

	var load = function (callback, businessUnitId, siteId) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "Teams/GetOutOfAdherenceForTeamsOnSite?siteId=" + siteId,
			success: function (data) {
				for (var i = 0; i < data.length; i++) {
					callback(mapAsNotification(data[i]));
				}
			}
		});
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, siteId, subscriptionDone) {
			unsubscribeAdherence();

			var poll = function() {
				load(callback, businessUnitId, siteId);
			};
			setTimeout(poll, 100);
			teamAdherencePoller = setInterval(poll, 5000);

			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
