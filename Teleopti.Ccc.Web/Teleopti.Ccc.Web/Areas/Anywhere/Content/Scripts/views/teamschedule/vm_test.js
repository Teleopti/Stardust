define(['buster', 'views/teamschedule/vm', 'shared/timezone-current'],
	function (buster, viewModel, timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');
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
				var data = {
					Schedules: [
						{
							PersonId: 1,
							Projection: [
								{
									Start: '2014-06-16 12:00',
									Minutes: 60
								}
							]
						}
					]
				};

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
				var data = {
					Schedules: [
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
						}
					]
				};

				vm.UpdateSchedules(data);

				setTimeout(function () {
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

				var data = {
					Schedules: [
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
					]
				};

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
				var data = {
					Schedules: [
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
					]
				};

				vm.UpdateSchedules(data);

				assert.equals(vm.PreSelectedPersonId(), 1);
				assert.equals(vm.Persons()[0].Selected(), true);
			},

			// Will fail without PBI #31897
			"should be able to preselect person and layer": function () {
				var vm = new viewModel();

				vm.Resources = { MyTeam_MakeTeamScheduleConsistent_31897: true };

				vm.SetViewOptions({
					date: "20150214",
					personid: "1",
					selectedStartMinutes: "870" // 14 * 60 + 30 <-- 14:30
				});
				var data = {
					"Schedules": [
						{
							"PersonId": "1",
							"Projection": [
								{
									"Start": "2015-02-14 9:45"
								}, {
									"Start": "2015-02-14 14:30"
								}
							]
						}
					]
				};

				vm.UpdateSchedules(data);

				assert.equals(vm.PreSelectedPersonId(), 1);
				assert.equals(vm.PreSelectedStartMinute(), 870);

				var expectedSelectedPerson = vm.Persons()[0];
				assert.equals(expectedSelectedPerson.Selected(), true);

				var expectedSelectedLayer = expectedSelectedPerson.Shifts()[0].Layers()[1];
				assert.equals(expectedSelectedLayer.Selected(), true);
			},

			"should show timeline correct for daylight saving time begin boundary day": function (done) {
				var vm = new viewModel();

				vm.SetViewOptions({
					date: '20150328'
				});

				var data = {
					BaseDate: "2015-03-28",
					Schedules: [
						{
							PersonId: "1",
							Date: "2015-03-28",
							Projection: [
								{
									Start: "2015-03-28 23:00",
									Minutes: 480
								}
							]
						}
					]
				};

				vm.UpdateSchedules(data);

				setTimeout(function () {
					assert.equals(vm.TimeLine.StartTime(), "23:00");
					assert.equals(vm.TimeLine.EndTime(), "08:00");

					var actualCount = 0;
					var times = vm.TimeLine.Times();
					for (var i = 0; i < times.length; i++) {
						if (times[i].Time == "02:00") {
							actualCount++;
						}
					}
					assert.equals(actualCount, 0);

					done();
				}, 2);
			},

			"should show timeline correct for daylight saving time end boundary day": function (done) {
				var vm = new viewModel();

				vm.SetViewOptions({
					date: '20141025'
				});

				var data = {
					BaseDate: "2014-10-25",
					Schedules: [
						{
							PersonId: "1",
							Date: "2014-10-25",
							Projection: [
								{
									Start: "2014-10-25 22:00",
									Minutes: 480
								}
							]
						}
					]
				};

				vm.UpdateSchedules(data);

				setTimeout(function () {
					assert.equals(vm.TimeLine.StartTime(), "22:00");
					assert.equals(vm.TimeLine.EndTime(), "05:00");

					var actualCount = 0;
					var times = vm.TimeLine.Times();
					for (var i = 0; i < times.length; i++) {
						if (times[i].Time == "02:00") {
							actualCount++;
						}
					}
					assert.equals(actualCount, 2);

					done();
				}, 2);
			}
		});
	};
});