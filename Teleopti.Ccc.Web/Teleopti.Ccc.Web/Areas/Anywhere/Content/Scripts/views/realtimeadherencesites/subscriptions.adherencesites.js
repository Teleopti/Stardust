define([
	'jquery',
	'ajax'
], function (
	$,
	ajax
	) {
	var poller = null;
	var unsubscribeAdherence = function () {
		if (!poller)
			return;
		clearInterval(poller);
		poller = null;
	};

	var mapAsNotification = function (data) {
		return {
			DomainId: data.Id,
			BinaryData: JSON.stringify(data)
		}
	}

	var load = function (callback, businessUnitId) {
		ajax.ajax({
			headers: { 'X-Business-Unit-Filter': businessUnitId },
			url: "api/Sites/GetOutOfAdherenceForAllSites",
			success: function (data) {
				for (var i = 0; i < data.length; i++) {
					callback(mapAsNotification(data[i]));
				}
			}
		});
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, subscriptionDone) {
			unsubscribeAdherence();

			var poll = function () {
				load(callback, businessUnitId);
			};
			setTimeout(poll, 100);
			poller = setInterval(poll, 5000);

			subscriptionDone();
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
