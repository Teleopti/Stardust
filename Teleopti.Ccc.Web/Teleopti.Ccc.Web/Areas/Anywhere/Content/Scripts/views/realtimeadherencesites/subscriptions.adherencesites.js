define([
	'resources',
	'views/realtimeadherencesites/subscriptions.adherencesites.broker',
	'views/realtimeadherencesites/subscriptions.adherencesites.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NewEventHangfireRTA_34333)
		return poller;
	return broker;

});
