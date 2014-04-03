
define([
    'buster',
    'views/diagnosis/vm'
], function (
    buster,
    viewModel
    ) {
	return function () {

		buster.testCase("real time adherence view model", {
			"should have resources available": function () {
				require('resources').ATextResources = "text";
				var vm = new viewModel();
				assert.defined(vm.resources.ATextResources);
			},
			"should show description": function () {
				var vm = new viewModel();
				assert.defined(vm.description);
			}

		});

	};
});