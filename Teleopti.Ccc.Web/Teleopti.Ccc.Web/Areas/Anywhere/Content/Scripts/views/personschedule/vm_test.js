define(['buster', 'views/personschedule/vm'],
	function (buster, viewModel) {
	return function () {

		buster.testCase("person schedule viewmodel", {

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
					}
				];

				vm.UpdateSchedules(data);

				setTimeout(function() {
					assert.equals(vm.TimeLine.StartTime(), "12:00");
					assert.equals(vm.TimeLine.EndTime(), "13:00");
					done();
				},2);
			},

			"should not consider nightshifts from yesterday when creating timeline": function (done) {
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
							},
							{
								Start: '2014-06-16 01:00',
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
				}, 2);

			},
			
			"should get the selected layer from url": function () {

				var vm = new viewModel();
				vm.SetViewOptions({
					id: 1,
					date: '20131118'
				});
				var data = [
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2013-11-18 14:00',
								Minutes: 240
							}
						]
					}
				];
				vm.UpdateSchedules(data);
				vm.SelectedStartMinutes(840);

				assert.equals(vm.SelectedLayer().StartMinutes(), vm.SelectedStartMinutes());
			},

			"should set move activity form when updating data": function () {

				var vm = new viewModel();
				vm.SetViewOptions({
					id: 1,
					date: '20131118'
				});
				var data = [
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2013-11-18 14:00',
								Minutes: 240,
								ActivityId:"guid"
							}
						]
					}
				];
				vm.SelectedStartMinutes(840);
				vm.UpdateData({ PersonId: 1 });
				vm.UpdateSchedules(data);

				assert.equals(vm.MoveActivityForm.PersonId(), 1);
				assert.equals(vm.MoveActivityForm.ScheduleDate().diff(moment('2013-11-18')), 0);
				assert.equals(vm.MoveActivityForm.OldStartMinutes(), vm.SelectedStartMinutes());
				assert.equals(vm.MoveActivityForm.ProjectionLength(), data[0].Projection[0].Minutes);
				assert.equals(vm.MoveActivityForm.ActivityId(), data[0].Projection[0].ActivityId);
			}

		});

	};
});