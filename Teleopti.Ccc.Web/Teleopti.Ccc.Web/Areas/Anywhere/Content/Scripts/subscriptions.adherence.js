define([
				'jquery',
				'messagebroker',
				'signalrhubs',
				'helpers',
				'errorview'
], function (
			$,
			messagebroker,
			signalRHubs,
			helpers,
			errorview
	) {

	var startPromise = messagebroker.start();

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
