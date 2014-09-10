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

			"should exclude tomorrows day off from timeline": function (done) {
				var vm = new viewModel();

				vm.SetViewOptions({
					date: '20140616'
				});
				var data = [
				{
					Offset: moment('2014-06-16'),
					PersonId: 1,
					Projection: [
						{
							Start: '2014-06-16 12:00',
							Minutes: 540
						}
					],
					IsFullDayAbsence: true,
				},
				{
					Offset: moment('2014-06-16'),
					PersonId: 1,
					Projection: [
						{
							Start: '2014-06-17 12:00',
							Minutes: 540
						}
					],
					IsFullDayAbsence: true,
					DayOff: {
						DayOffName: "DayOff",
						Start: "2014-06-17 00:00",
						Minutes: 1440
					}
				}];

				vm.UpdateSchedules(data);

				setTimeout(function () {
					assert.equals(vm.TimeLine.Times().length, 10);
					assert.equals(vm.TimeLine.StartTime(), "12:00");
					assert.equals(vm.TimeLine.EndTime(), "21:00");
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
						Date: '2014-06-16',
						PersonId: 1,
						Projection: [
							{
								Start: '2014-06-16 12:00',
								Minutes: 60
							}
						]
					},
					{
						Date: '2014-06-15',
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

			},

			"should be able to preselect person": function () {
				var vm = new viewModel();

				vm.SetViewOptions({
					personid: 1
				});
				var data = [
					{
						Date: '2014-06-16',
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

				assert.equals(vm.PreSelectedPersonId(), 1);
				assert.equals(vm.Persons()[0].Selected(), true);
			},
		});

	};
});