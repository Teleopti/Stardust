define([
		'jquery',
		'subscriptions.groupschedule',
		'subscriptions.personschedule',
		'subscriptions.staffingmetrics',
		'subscriptions.adherence'
	], function(
		$,
		groupschedule,
		personschedule,
		staffingmetrics,
		adherence
	) {

		return {
			start: function () {
				return $.when(
					groupschedule.start(),
					personschedule.start(),
					staffingmetrics.start(),
					adherence.start()				
				);
			}
		};

	});
