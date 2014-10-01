require.config(requireconfiguration);

require([
		'vm_test', 'healthvm_test'
], function () {
	for (var i = 0, j = arguments.length; i < j; i++) {
		arguments[i]();
	}
});
