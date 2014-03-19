define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var startPromise;

	var siteAdherenceSubscription = null;

	var unsubscribeAdherence = function () {
		if (!siteAdherenceSubscription)
			return;
		startPromise.done(function () {
			messagebroker.unsubscribe(siteAdherenceSubscription);
			siteAdherenceSubscription = null;
		});
	};

	return {
		start: function () {
			startPromise = messagebroker.start();
			return startPromise;
		},

		subscribeAdherence: function (callback) {
			unsubscribeAdherence();
			startPromise.done(function () {
				siteAdherenceSubscription = messagebroker.subscribe({
					domainType: 'SiteAdherenceMessage',
					callback: callback
				});
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
