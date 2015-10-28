define([
	'views/realtimeadherencesites/subscriptions.adherencesites',
	'views/realtimeadherenceteams/subscriptions.adherenceteams',
	'views/realtimeadherenceagents/subscriptions.adherenceagents'
], function(
	sites,
	teams,
	agents
	) {

	return {
		unsubscribeAdherence: function() {
			sites.unsubscribeAdherence();
			teams.unsubscribeAdherence();
			agents.unsubscribeAdherence();
		}
	}
});
