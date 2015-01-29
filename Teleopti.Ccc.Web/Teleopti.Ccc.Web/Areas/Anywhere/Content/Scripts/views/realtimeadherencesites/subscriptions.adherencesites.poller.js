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

	var mapAsNotification = function(data) {
		return {
			DomainId: data.Id,
			BinaryData: JSON.stringify(data)
		}
	}

	return {
		subscribeAdherence: function (callback, businessUnitId, subscriptionDone) {
			unsubscribeAdherence();

			siteAdherencePoller = setInterval(function() {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					url: "Sites/GetOutOfAdherenceForAllSites",
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
