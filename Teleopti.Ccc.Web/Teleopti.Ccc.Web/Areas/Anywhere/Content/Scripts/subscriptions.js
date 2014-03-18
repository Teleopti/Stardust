define([
		'jquery',
		'subscriptions.groupschedule',
		'subscriptions.personschedule',
		'subscriptions.staffingmetrics',
		'subscriptions.adherenceteams',
		'subscriptions.adherencesites'
	], function(
		$,
		groupschedule,
		personschedule,
		staffingmetrics,
		adherenceteams,
		adherencesites
	) {

		return {
			start: function () {
				return $.when(
					groupschedule.start(),
					personschedule.start(),
					staffingmetrics.start(),
					adherenceteams.start(),
					adherencesites.start()
				);
			}
		};

	});
