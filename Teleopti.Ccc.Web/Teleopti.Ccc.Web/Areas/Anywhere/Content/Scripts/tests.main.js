require.config(requireconfiguration);

define('resources', {
		//TimeFormatForMoment: "HH:mm",
		FixedDateTimeFormatForMoment: "YYYY-MM-DD HH:mm",
		FixedTimeFormatForMoment: "HH:mm"
	});

define('window', {
	setLocationHash: function () { },
	locationReplace: function () { },
});

var testCases = [
	'tests',
	'views/personschedule/vm_test',
	'views/realtimeadherencesites/vm_test',
	'views/realtimeadherenceteams/vm_test',
	'views/realtimeadherenceagents/vm_test',
	'views/realtimeadherenceteams/team_test',
	'helpers_test'
];

require(testCases, function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
			arguments[i]();
	}
});
