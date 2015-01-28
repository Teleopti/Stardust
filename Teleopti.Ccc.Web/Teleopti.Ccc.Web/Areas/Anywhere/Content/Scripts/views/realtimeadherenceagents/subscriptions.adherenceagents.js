define([
	'resources',
	'views/realtimeadherenceagents/subscriptions.adherenceagents.broker',
	'views/realtimeadherenceagents/subscriptions.adherenceagents.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NoBroker_31237)
		return poller;
	return broker;

});
