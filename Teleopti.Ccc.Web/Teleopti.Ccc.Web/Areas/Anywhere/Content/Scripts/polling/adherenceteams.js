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

	var mapToNotificationStyle = function(data) {
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

			siteAdherencePoller = setInterval(function() {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					url: "Teams/GetOutOfAdherenceForTeamsOnSite?siteId=" + siteId,
					success: function (data) {
						for (var i = 0; i < data.length; i++) {
							callback(mapToNotificationStyle(data[i]));
						}
					}
				});
			}, 5000);

			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
