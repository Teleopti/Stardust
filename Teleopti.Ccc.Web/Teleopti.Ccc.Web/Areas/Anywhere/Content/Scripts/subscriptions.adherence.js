define([
				'jquery',
				'messagebroker',
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

		subscribeAdherence: function () {
			console.log('starting subscribe');
			unsubscribeAdherence();
			startPromise.done(function () {
				console.log('startpromise done!');
				siteAdherenceSubscription = messagebroker.subscribe({
					domainType: 'SiteAdherenceMessage',
					callback: function (notification) {
						console.log('message arived' + notification);
						$('body').append("arrived!");
					}
				});
			});
		},

		unsubscribeAdherence: unsubscribeAdherence
	};

});
