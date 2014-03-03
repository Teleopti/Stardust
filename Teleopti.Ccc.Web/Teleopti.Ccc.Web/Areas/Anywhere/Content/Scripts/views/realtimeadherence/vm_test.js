require(['buster', 'views/realtimeadherence/vm'], function (buster, viewModel) {

	buster.testCase("realTimeAdherence_viewModel", {
		"can create instance": function () {
			var vm = viewModel();
			assert.defined(vm);
		}
	});
});