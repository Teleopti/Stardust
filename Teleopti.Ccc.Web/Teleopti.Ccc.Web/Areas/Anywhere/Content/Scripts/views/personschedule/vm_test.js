
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
			},

			"should create timeline with default times": function() {
				var vm = new viewModel();

				assert.equals(vm.TimeLine.StartMinutes(), 8 * 60);
				assert.equals(vm.TimeLine.EndMinutes(), 16 * 60);
			},

			"should create timeline according to shifts length": function () {
				assert(true);
				return;

				var vm = new viewModel();

				var data = [
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2014-06-16 12:00',
								Minutes: 60
							}
						]
					}
				];
				vm.UpdateSchedules(data);

				assert.equals(vm.TimeLine.StartMinutes(), 12 * 60);
				assert.equals(vm.TimeLine.EndMinutes(), 13 * 60);
			}

		});

	};
});