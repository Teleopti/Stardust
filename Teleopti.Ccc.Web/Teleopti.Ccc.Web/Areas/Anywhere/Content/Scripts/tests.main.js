require.config(requireconfiguration);

define('resources', {});

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
		'views/personschedule/vm_test',
];
		'helpers_test'

require(testCases, function () {
	var query = location.search.substring(1);
	for (var i = 0, j = arguments.length; i < j; i++) {
		if (query == '' || testCases[i].indexOf(query) > 0)
			arguments[i]();
	}
});
