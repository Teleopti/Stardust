require.config(requireconfiguration);

require([
		'vm_test'
], function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
		arguments[i]();
	}
});
