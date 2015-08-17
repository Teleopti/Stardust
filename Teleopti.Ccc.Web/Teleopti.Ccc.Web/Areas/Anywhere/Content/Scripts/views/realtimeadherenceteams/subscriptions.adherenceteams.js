define([
	'resources',
	'views/realtimeadherenceteams/subscriptions.adherenceteams.broker',
	'views/realtimeadherenceteams/subscriptions.adherenceteams.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NewEventHangfireRTA_34333)
		return poller;
	return broker;

});
