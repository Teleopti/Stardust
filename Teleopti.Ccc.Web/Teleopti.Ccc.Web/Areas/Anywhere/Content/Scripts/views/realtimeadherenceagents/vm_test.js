define(['buster', 'views/realtimeadherenceagents/vm'], function (buster, viewModel) {
	return function () {
		
		buster.testCase("real time adherence agents viewmodel", {
			"should have no agents if none filled": function () {
				var vm = viewModel();
				assert.equals(vm.agents(), []);
			},

		});		
	};
});