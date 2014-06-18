define(['buster', 'views/teamschedule/vm'],
	function (buster, viewModel) {
	return function () {

		buster.testCase("team schedule viewmodel", {

			"should create viewmodel": function () {
				var vm = new viewModel();
				assert(vm);
			},
			
			"should create timeline with default times": function() {
				var vm = new viewModel();

				assert.equals(vm.TimeLine.StartTime(), "08:00");
				assert.equals(vm.TimeLine.EndTime(), "16:00");
			},

			"should create timeline according to shifts length": function (done) {
				var vm = new viewModel();

				vm.SetViewOptions({
					date: '20140616'
				});
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

				setTimeout(function () {
					assert.equals(vm.TimeLine.StartTime(), "12:00");
					assert.equals(vm.TimeLine.EndTime(), "13:00");
					done();
				},2);
			},

			"should consider nightshifts from yesterday when creating timeline" : function(done) {
				var vm = new viewModel();

				vm.SetViewOptions({
					id: 1,
					date: '20140616'
				});

				var data = [
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2014-06-16 12:00',
								Minutes: 60
							}
						]
					},
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2014-06-15 22:00',
								Minutes: 180
							}
						]
					}
				];

				vm.UpdateSchedules(data);

				setTimeout(function () {
					assert.equals(vm.TimeLine.StartTime(), "00:00");
					assert.equals(vm.TimeLine.EndTime(), "13:00");
					done();
				}, 2);

			}
		});

	};
});