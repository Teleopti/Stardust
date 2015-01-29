define([
	'resources',
	'views/realtimeadherenceteams/subscriptions.adherenceteams.broker',
	'views/realtimeadherenceteams/subscriptions.adherenceteams.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NoBroker_31237)
		return poller;
	return broker;

});
