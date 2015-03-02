define(['buster', 'views/personschedule/vm','lazy','shared/timezone-current'],
	function (buster, viewModel,lazy,timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');

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

					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					setTimeout(function () {
						assert.equals(vm.TimeLine.StartTime(), "12:00");
						assert.equals(vm.TimeLine.EndTime(), "13:00");
						done();
					}, 2);
				},

				"should create timeline for other timezone": function (done) {
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
							}
						]
					};

					vm.AddingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Istanbul' });
					vm.UpdateSchedules(data);

					setTimeout(function () {
						assert.equals(vm.TimeLine.StartTime(), "12:00");
						assert.equals(vm.TimeLine.EndTime(), "13:00");
						assert.equals(vm.TimeLine.StartTimeOtherTimeZone(), "13:00");
						assert.equals(vm.TimeLine.EndTimeOtherTimeZone(), "14:00");
						done();
					}, 2);
				},

				"should not consider nightshifts from yesterday when creating timeline": function (done) {
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
									},
									{
										Start: '2014-06-16 01:00',
										Minutes: 60
									}
								]
							}
						]
					};

					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
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
						date: '20131118',
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 240
									}
								]
							}
						]
					};
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					assert.equals(vm.SelectedLayer().StartMinutes(), vm.SelectedStartMinutes());
				},

				"should set move activity form when updating data": function () {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 240,
										ActivityId: "guid"
									}
								]
							}
						]
					};
					vm.MovingActivity(true);
					vm.UpdateData({ PersonId: 1 });
					vm.UpdateSchedules(data);

					assert.equals(vm.MoveActivityForm.PersonId(), 1);
					assert.equals(vm.MoveActivityForm.GroupId(), 2);
					assert.equals(vm.MoveActivityForm.ScheduleDate().format('YYYY-MM-DD HH:mm'),'2013-11-18 00:00');
					assert.equals(vm.MoveActivityForm.OldStartMinutes(), vm.SelectedStartMinutes());
					assert.equals(vm.MoveActivityForm.ProjectionLength(), data.Schedules[0].Projection[0].Minutes);
				},

				"should update starttime when DisplayedStartTime changes": function() {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 60
									},
									{
										Start: '2013-11-18 15:00',
										Minutes: 420
									}
								]
							}
						]
					};
					vm.MovingActivity(true);
					vm.UpdateData({ PersonId: 1, Date: moment('20131118', 'YYYYMMDD'), IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					var momentExpected = moment('2013-11-18 15:00', 'YYYY-MM-DD HH:mm');
					vm.MoveActivityForm.DisplayedStartTime('15:00');
					assert.equals(vm.MoveActivityForm.StartTime().format('HH:mm'), momentExpected.format('HH:mm'));
				},

				"should update DisplayedStartTime when startTime changes": function () {

					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00'
									}
								]
							}
						]
					};
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
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
						groupid: 2,
						minutes: 340
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 240,
										ActivityId: "guid"
									}
								]
							}
						]
					};

					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					//just verifies it doesn't throw
					assert.equals(vm.MoveActivityForm.PersonId(), 1);
				},

				"should change the active layer start time when the move activity form start time changes": function () {
					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 240,
										ActivityId: "guid"
									},
									{
										Start: '2013-11-18 18:00',
										Minutes: 240,
										ActivityId: "guid"
									}
								]
							}
						]
					};
					vm.MovingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					var selectedLayer = vm.SelectedLayer();
					var momentExpected = moment('2013-11-18 15:00', 'YYYY-MM-DD HH:mm');

					vm.MoveActivityForm.DisplayedStartTime('15:00');

					assert.equals(selectedLayer.StartTime(), momentExpected.format('HH:mm'));
				},

				"should calculate shifts width lower than timeline width when groupmates are displayed": function () {
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
										Start: '2014-06-16 8:00',
										Minutes: 60
									}
								]
							},
							{
								Date: '2014-06-16',
								PersonId: 2,
								Projection: [
									{
										Start: '2014-06-16 16:00',
										Minutes: 20
									}
								]
							}
						]
					};
					vm.setTimelineWidth(600);
					vm.AddingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					assert.equals(vm.Layers().size(), 2);

					vm.Layers().forEach(function (layer) {
						var startPX = layer.StartPixels();
						var lengthPX = layer.LengthPixels();
						assert.isTrue(vm.TimeLine.WidthPixels() >= (startPX + lengthPX));
					});
					
				},

				"should convert my local times to agent's timezone for display when adding activity": function () {
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
										Start: '2014-06-16 8:00',
										Minutes: 60
									}
								]
							}
						]
					};
					vm.AddingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Istanbul' });
					vm.UpdateSchedules(data);

					assert.equals(vm.Layers().size(), 1);

					vm.AddActivityForm.StartTime('09:00');
					vm.AddActivityForm.EndTime('11:00');

					assert.equals(vm.AddActivityForm.StartTimeOtherTimeZone(), '10:00');
					assert.equals(vm.AddActivityForm.EndTimeOtherTimeZone(), '12:00');
				},

				"should convert my local times to agent's timezone for display when removing absence": function () {
					var vm = new viewModel();

					vm.SetViewOptions({
						id: 1,
						date: '20140616'
					});
					
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Istanbul', PersonAbsences: [{ StartTime: '2014-06-16 08:00', EndTime: '2014-06-16 10:00' }] });
					vm.UpdateSchedules([]);

					assert.equals(vm.Absences().length, 1);

					var absence = vm.Absences()[0];
					assert.equals(absence.StartDateTime, '2014-06-16 08:00');
					assert.equals(absence.EndDateTime, '2014-06-16 10:00');
					assert.equals(absence.StartTimeOtherTimeZone(), '2014-06-16 09:00');
					assert.equals(absence.EndTimeOtherTimeZone(), '2014-06-16 11:00');
				},

				"should convert my local times to agent's timezone for display when adding intraday absence": function () {
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
										Start: '2014-06-16 8:00',
										Minutes: 60
									}
								]
							}
						]
					};
					vm.setTimelineWidth(600);
					vm.AddingIntradayAbsence(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Istanbul' });
					vm.UpdateSchedules(data);

					vm.AddIntradayAbsenceForm.StartTime('09:00');
					vm.AddIntradayAbsenceForm.EndTime('11:00');

					assert.equals(vm.AddIntradayAbsenceForm.StartTimeOtherTimeZone(), '10:00');
					assert.equals(vm.AddIntradayAbsenceForm.EndTimeOtherTimeZone(), '12:00');
				},


				"should convert my local times to agent's timezone for display when moving activity": function () {
					var vm = new viewModel();

					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});

					var data = {
						Schedules: [
							{
								PersonId: 1,
								Date: '2013-11-18',
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 240,
										ActivityId: "guid"
									},
									{
										Start: '2013-11-18 18:00',
										Minutes: 240,
										ActivityId: "guid"
									}
								]
							}
						]
					};

					vm.MovingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Istanbul' });
					vm.UpdateSchedules(data);

					vm.MoveActivityForm.DisplayedStartTime('15:00');

					assert.equals(vm.MoveActivityForm.StartTimeOtherTimeZone(), '16:00');
				},


				"should not input a start time that makes the layer outside of the shift in move activity form" : function() {
					var vm = new viewModel();
					vm.SetViewOptions({
						id: 1,
						date: '20131118',
						groupid: 2,
						minutes: 840
					});
					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 14:00',
										Minutes: 60
									}
								]
							}
						]
					};
					vm.MovingActivity(true);
					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
					vm.UpdateSchedules(data);

					var momentInput = moment('13:00', 'HH:mm');
					var expected = '14:00';
					vm.MoveActivityForm.DisplayedStartTime(momentInput);
					assert.equals(vm.MoveActivityForm.DisplayedStartTime(), expected);

					momentInput = moment('14:30', 'HH:mm');
					vm.MoveActivityForm.DisplayedStartTime(momentInput);
					assert.equals(vm.MoveActivityForm.DisplayedStartTime(), expected);
				},

				"should not display duplicated teammates" : function() {

					var vm = new viewModel();
					vm.SetViewOptions({
						date: '20131118',
					});

					vm.AddingActivity(true); //for showing the other agents in team

					vm.PersonId('1');

					var data = {
						Schedules: [
							{
								PersonId: '2'
							},
							{
								PersonId: '3'
							},
							{
								PersonId: '3'
							},
							{
								PersonId: '3'
							},
							{
								PersonId: '2'
							}
						]
					};

					vm.UpdateSchedules(data);

					assert.equals(3, vm.Persons().length);
				},


				"should not move layer which makes the shift to another day in move activity form": function () {
				    var vm = new viewModel();
				    vm.SetViewOptions({
				        id: 1,
				        date: '20131118',
				        groupid: 2,
				        minutes: 1380
				    });
				    var data = {
					    Schedules: [
						    {
							    Date: '2013-11-18',
							    PersonId: 1,
							    Projection: [
								    {
									    Start: '2013-11-18 23:00',
									    Minutes: 60
								    },
								    {
									    Start: '2013-11-19 00:00',
									    Minutes: 60
								    }
							    ]
						    }
					    ]
				    };
				    vm.MovingActivity(true);
				    vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
				    vm.UpdateSchedules(data);

				    var momentInput = moment('23:30', 'HH:mm');
				    vm.MoveActivityForm.DisplayedStartTime(momentInput);
				    var result = vm.MoveActivityForm.isMovingToAnotherDay();
				    assert.equals(result, false);

				    momentInput = moment('00:00', 'HH:mm');
				    vm.MoveActivityForm.DisplayedStartTime(momentInput);
				    result = vm.MoveActivityForm.isMovingToAnotherDay();
				    assert.equals(result, true);

				},
				"should select layer starts at midnight in move activity form": function () {
				    var vm = new viewModel();

					vm.Resources = { MyTeam_MakeTeamScheduleConsistent_31897: false };

				    vm.SetViewOptions({
				        id: 1,
				        date: '20131118',
				        groupid: 2,
				        minutes: 0
				    });

					var data = {
						Schedules: [
							{
								Date: '2013-11-18',
								PersonId: 1,
								Projection: [
									{
										Start: '2013-11-18 23:00',
										Minutes: 60
									},
									{
										Start: '2013-11-19 00:00',
										Minutes: 60
									},
									{
										Start: '2013-11-19 01:00',
										Minutes: 60
									}
								]
							}
						]
					};
				    vm.MovingActivity(true);
				    vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
				    vm.UpdateSchedules(data);

				    var selectedLayer = vm.SelectedLayer();
				    var momentExpected = moment('2013-11-19 00:00', 'YYYY-MM-DD HH:mm');

				    assert.equals(selectedLayer.StartTime(), momentExpected.format('HH:mm'));
				},

				"should show timeline correct for daylight saving time begin boundary day": function (done) {
					var vm = new viewModel();

					vm.SetViewOptions({
						id: 1,
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

					vm.UpdateData({ PersonId: 1, IanaTimeZoneOther: 'Europe/Berlin' });
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
						id: 1,
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
		}
	}
);