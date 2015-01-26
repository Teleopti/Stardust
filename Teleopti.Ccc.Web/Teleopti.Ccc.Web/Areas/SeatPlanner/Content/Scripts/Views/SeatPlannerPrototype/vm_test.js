define(['buster', 'views/SeatPlannerPrototype/vm', 'shared/timezone-current'],
	function (buster, viewModel, timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');
		return function () {

			buster.testCase("Seat Planner Prototype viewmodel", {

				"should create viewmodel": function () {
					var vm = new viewModel();
					assert(vm);
				},
				"should have start and end dates": function () {
					var vm = new viewModel();
					assert(vm.StartDate != undefined);
					assert(vm.EndDate != undefined);
					assert(vm.EndDate().day() - vm.StartDate().day() == 1);

				}
			});
		};
	});