require.config(requireconfiguration);

require([
		'tests',
		'views/realtimeadherencesites/vm_test',
		'views/realtimeadherenceteams/vm_test'
], function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
		arguments[i]();
	}
});
