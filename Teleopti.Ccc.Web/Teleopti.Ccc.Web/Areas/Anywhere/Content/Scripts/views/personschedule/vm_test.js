
define([
    'buster',
    'views/personschedule/vm'
], function (
    buster,
    viewModel
    ) {
	return function () {

		buster.testCase("personschedule viewmodel", {
			"should create viewmodel": function () {
				var vm = new viewModel();
				assert(vm);
			}

		});

	};
});