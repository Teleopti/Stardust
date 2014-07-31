define([
		'jquery',
		'subscriptions.groupschedule',
		'subscriptions.personschedule',
		'subscriptions.staffingmetrics',
		'subscriptions.adherenceteams',
		'subscriptions.adherencesites',
		'subscriptions.adherenceagents',
		'subscriptions.trackingmessages'
	], function(
		$,
		groupschedule,
		personschedule,
		staffingmetrics,
		adherenceteams,
		adherencesites,
		adherenceagents,
		trackingmessages
	) {

		return {
			start: function () {
				return $.when(
					groupschedule.start(),
					personschedule.start(),
					staffingmetrics.start(),
					adherenceteams.start(),
					adherencesites.start(),
					adherenceagents.start(),
					trackingmessages.start()
				);
			}
		};

	});
