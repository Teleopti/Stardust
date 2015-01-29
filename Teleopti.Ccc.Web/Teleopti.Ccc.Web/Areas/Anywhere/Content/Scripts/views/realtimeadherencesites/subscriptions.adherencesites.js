define([
	'resources',
	'views/realtimeadherencesites/subscriptions.adherencesites.broker',
	'views/realtimeadherencesites/subscriptions.adherencesites.poller'
], function (
	resources,
	broker,
	poller
	) {

	if (resources.RTA_NoBroker_31237)
		return poller;
	return broker;

});
