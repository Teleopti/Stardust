define([
		'jquery',
		'subscriptions.groupschedule',
		'subscriptions.personschedule',
		'subscriptions.staffingmetrics',
		'subscriptions.adherenceteams',
		'subscriptions.adherencesites',
		'subscriptions.adherenceagents'
	], function(
		$,
		groupschedule,
		personschedule,
		staffingmetrics,
		adherenceteams,
		adherencesites,
		adherenceagents
	) {

		return {
			start: function () {
				return $.when(
					groupschedule.start(),
					personschedule.start(),
					staffingmetrics.start(),
					adherenceteams.start(),
					adherencesites.start(),
					adherenceagents.start()
				);
			}
		};

	});
