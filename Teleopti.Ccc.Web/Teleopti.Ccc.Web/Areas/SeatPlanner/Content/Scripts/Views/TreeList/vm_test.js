define(['buster', 'views/TreeList/vm', 'shared/timezone-current'],
	function (buster, viewModel, timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');
		return function () {

			buster.testCase("Tree List viewmodel", {

				"should create viewmodel": function () {
					var vm = new viewModel();
					assert(vm);
				}
			});
		};
	});