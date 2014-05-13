require.config(requireconfiguration);

define('resources', {});

define('window', {
	setLocationHash: function () { },
	locationReplace: function () { },
});

require([
		'tests',
		'views/realtimeadherencesites/vm_test',
		'views/realtimeadherenceteams/vm_test',
		'views/realtimeadherenceagents/vm_test'
], function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
		arguments[i]();
	}
});
