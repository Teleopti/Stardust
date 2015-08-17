define([
	'resources',
	'views/realtimeadherenceagents/subscriptions.adherenceagents.broker',
	'views/realtimeadherenceagents/subscriptions.adherenceagents.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NewEventHangfireRTA_34333)
		return poller;
	return broker;

});
