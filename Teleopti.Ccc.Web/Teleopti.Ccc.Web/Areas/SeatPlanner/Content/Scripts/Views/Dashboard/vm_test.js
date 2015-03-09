define(['buster', 'views/SeatMap/vm', 'shared/timezone-current'],
	function (buster, viewModel, timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');
		return function () {

			buster.testCase("Seat Map viewmodel", {

				"should create viewmodel": function () {
					var vm = new viewModel();
					assert(vm);
				}
			});
		};
	});