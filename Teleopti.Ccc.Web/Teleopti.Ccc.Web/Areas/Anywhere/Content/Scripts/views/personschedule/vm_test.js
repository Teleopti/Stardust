﻿define(['buster', 'views/personschedule/vm'],
	function (buster, viewModel) {
		return function () {

			buster.testCase("person schedule viewmodel", {
				"should create viewmodel": function () {
					var vm = new viewModel();
					assert(vm);
				},

				"should create timeline with default times": function () {
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

					setTimeout(function () {
						assert.equals(vm.TimeLine.StartTime(), "12:00");
						assert.equals(vm.TimeLine.EndTime(), "13:00");
						done();
					}, 2);
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

					assert.equals(vm.SelectedLayer()().StartMinutes(), vm.SelectedStartMinutes());
				},

				"should set move activity form when updating data": function () {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2
					});
					var data = [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2013-11-18 14:00',
									Minutes: 240,
									ActivityId: "guid"
								}
							]
						}
					];
					vm.SelectedStartMinutes(840);
					vm.UpdateData({ PersonId: 1 });
					vm.UpdateSchedules(data);

					assert.equals(vm.MoveActivityForm.PersonId(), 1);
					assert.equals(vm.MoveActivityForm.GroupId(), 2);
					assert.equals(vm.MoveActivityForm.ScheduleDate().diff(moment('2013-11-18')), 0);
					assert.equals(vm.MoveActivityForm.OldStartMinutes(), vm.SelectedStartMinutes());
					assert.equals(vm.MoveActivityForm.ProjectionLength(), data[0].Projection[0].Minutes);
					assert.equals(vm.MoveActivityForm.ActivityId(), data[0].Projection[0].ActivityId);
				},

				"should update starttime when DisplayedStartTime changes": function () {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2
					});
					var data = [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2013-11-18 14:00'
								}
							]
						}
					];
					vm.SelectedStartMinutes(840);
					vm.UpdateData({ PersonId: 1, Date: moment('20131118', 'YYYYMMDD') });
					vm.UpdateSchedules(data);

					var momentExpected = moment('2013-11-18 15:00', 'YYYY-MM-DD HH:mm');
					vm.MoveActivityForm.DisplayedStartTime('15:00');
					assert.equals(vm.MoveActivityForm.StartTime().format(), momentExpected.format());
				},

				"should update DisplayedStartTime when startTime changes": function () {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2
					});
					var data = [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2013-11-18 14:00'
								}
							]
						}
					];
					vm.SelectedStartMinutes(840);
					vm.UpdateData({ PersonId: 1 });
					vm.UpdateSchedules(data);

					var expected = '15:00';
					var momentExpected = moment(expected, 'HH:mm');
					vm.MoveActivityForm.StartTime(momentExpected);
					assert.equals(vm.MoveActivityForm.DisplayedStartTime(), expected);
				},


				"should not try to select layer if layer not choosen": function () {
					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2
					});
					var data = [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2013-11-18 14:00',
									Minutes: 240,
									ActivityId: "guid"
								}
							]
						}
					];

					vm.SelectedStartMinutes(340);
					vm.UpdateData({ PersonId: 1 });
					vm.UpdateSchedules(data);

					//just verifies it doesn't throw
					assert.equals(vm.MoveActivityForm.PersonId(), 1);
				},

				"should change the active layer start time when the move activity form start time changes": function () {
					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2
					});
					var data = [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2013-11-18 14:00',
									Minutes: 240,
									ActivityId: "guid"
								}
							]
						}
					];

					vm.SelectedStartMinutes(840);
					vm.UpdateData({ PersonId: 1 });
					vm.UpdateSchedules(data);

					var selectedLayer = vm.SelectedLayer();
					var momentExpected = moment('2013-11-18 15:00', 'YYYY-MM-DD HH:mm');

					vm.MoveActivityForm.DisplayedStartTime('15:00');

					assert.equals(selectedLayer().StartTime(), momentExpected.format('HH:mm'));
				}
			});
		}
	}
);