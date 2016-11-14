(function() {
	'use strict';

	describe('teamschedule move activity validator tests', function() {
		var target, personSelection, scheduleMgmt;
		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(function() {
			module(function($provide) {
				$provide.service('Toggle', function () {
					return {
						WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305: true
					};
				});
				$provide.service('CurrentUserInfo',
					function() {
						return {
							CurrentUserInfo: function() {
								return { DefaultTimeZone: 'Asia/Hong_Kong' };
							}
						};
					});
			});
		});

		beforeEach(inject(function (PersonSelection, ScheduleManagement, MoveActivityValidator) {
			personSelection = PersonSelection;
			scheduleMgmt = ScheduleManagement;
			target = MoveActivityValidator;
		}));
		var scheduleDate = "2016-05-12";
		var schedule = {
			"PersonId": "221B-Baker-SomeoneElse",
			"Name": "SomeoneElse",
			"Date": scheduleDate,
			"Timezone": {
				IanaId: "Asia/Hong_Kong"
			},
			"Projection": [
				{
					"ShiftLayerIds": ["layer1"],
					"Color": "#80FF80",
					"Description": "Email",
					"Start": scheduleDate + " 07:00",
					"Minutes": 480
				}
			],
			"IsFullDayAbsence": false,
			"DayOff": null
		};

		it('should return false when moving to time changes the schedule start date', function() {
			
			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
			var newStartMoment = moment("2016-05-13 2:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
			expect(target.getInvalidPeopleNameList().indexOf('SomeoneElse') > -1).toEqual(true);
		});

		it('should return false when moving to time makes the schedule length longer than 36 hours', function () {
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 01:00",
						"Minutes": 480
					},
					{
						"ShiftLayerIds": ["layer2"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Phone",
						"Start": scheduleDate + " 10:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[1].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[1]);
			var newStartMoment = moment("2016-05-13 13:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
			expect(target.getInvalidPeopleNameList().indexOf('SomeoneElse') > -1).toEqual(true);
		});

		it('should return true when moving activity in previous day within shift', function () {
			var previousDay = "2016-05-11";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": previousDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": previousDay + " 22:00",
						"Minutes": 180
					}, {
						"ShiftLayerIds": ["layer2"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Short Break",
						"Start": scheduleDate + " 01:00",
						"Minutes": 60
					}, {
						"ShiftLayerIds": ["layer3"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 02:00",
						"Minutes": 120
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[1].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[1], personSchedule);
			var newStartMoment = moment(scheduleDate + " 00:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(true);
		});

		it('should return false when moving activity in previous day out of shift', function() {
			var previousDay = "2016-05-11";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": previousDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": previousDay + " 22:00",
						"Minutes": 180
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
			var newStartMoment = moment(scheduleDate + " 01:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
		});

		it('should return false when moving activity in next day out of shift', function() {
			var nextDay = "2016-05-13";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": nextDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": nextDay + " 01:00",
						"Minutes": 180
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
			var newStartMoment = moment(scheduleDate + " 01:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
		});

		it('should return true when moving activity in next day within shift', function() {
			var nextDay = "2016-05-13";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": nextDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": nextDay + " 01:00",
						"Minutes": 180
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
			var newStartMoment = moment(nextDay + " 02:00");

			var result = target.validateMoveToTime(newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(true);
		});

		it('should validate move to time when moving shift to time in current date', function () {
			var currentDate = "2016-05-13";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 08:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(scheduleDate + " 02:00");

			target.validateMoveToTimeForShift(newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(0);
		});

		it('should validate move to time when moving shift to time on next date', function () {
			var nextDay = "2016-05-13";
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 08:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(nextDay + " 02:00");

			target.validateMoveToTimeForShift(newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should invalidate shift-move if shift will conflict with shift of tomorrow after the move', function () {
			var nextDay = "2016-05-13";
			var scheduleToday = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 08:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			var scheduleNextDay = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": nextDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": nextDay + " 05:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday, scheduleNextDay], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(scheduleDate + " 23:00");

			target.validateMoveToTimeForShift(newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should invalidate shift-move if shift will conflict with shift of yesterday after the move', function () {
			var prevDay = "2016-05-11";
			var scheduleToday = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 13:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			var scheduleNextDay = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": prevDay,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": prevDay + " 23:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday, scheduleNextDay], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(scheduleDate + " 5:00");

			target.validateMoveToTimeForShift(newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

	});
})()
