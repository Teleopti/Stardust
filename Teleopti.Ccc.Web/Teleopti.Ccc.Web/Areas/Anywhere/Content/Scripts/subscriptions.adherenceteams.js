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

	return {
		start: function () {
			return $.Deferred().resolve();
		},

		subscribeAdherence: function (callback, businessUnitId, siteId, subscriptionDone) {
			unsubscribeAdherence();

			teamAdherencePoller = setInterval(function () {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					url: "Teams/GetOutOfAdherenceForTeamsOnSite?siteId=" + siteId,
					success: function (data) {
						for (var i = 0; i < data.length; i++) {
							callback(mapAsNotification(data[i]));
						}
					}
				});
			}, 5000);

			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
