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

		subscribeAdherence: function (callback, subscriptionDone) {
			unsubscribeAdherence();
			startPromise.done(function () {
				siteAdherenceSubscription = messagebroker.subscribe({
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
