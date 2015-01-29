define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var siteAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!siteAdherenceSubscription)
			return;
		messagebroker.unsubscribe(siteAdherenceSubscription);
		siteAdherenceSubscription = null;
	};

	return {
		subscribeAdherence: function (callback, businessUnitId, subscriptionDone) {
			unsubscribeAdherence();
			messagebroker.started.done(function () {
				siteAdherenceSubscription = messagebroker.subscribe({
					businessUnitId: businessUnitId,
					domainType: 'SiteAdherenceMessage',
					callback: callback
				});
				siteAdherenceSubscription.promise.done(function () {
					subscriptionDone();
				});
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
