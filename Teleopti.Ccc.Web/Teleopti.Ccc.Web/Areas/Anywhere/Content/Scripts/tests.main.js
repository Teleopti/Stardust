require.config(requireconfiguration);

require([
		'tests',
		'views/realtimeadherence/vm_test',
		'views/realtimeadherencesite/vm_test'
], function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
		arguments[i]();
	}
});
