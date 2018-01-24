﻿(function () {
	'use strict';

	describe('teamschedule move activity validator tests: ', function () {
		var target, personSelection, scheduleMgmt, fakeTeamsToggles, fakeTeamsPermissions;
		var defaultUserTimeZone = 'Asia/Hong_Kong';  //UTC+8

		var scheduleDate = "2016-05-12";
		var definitionSetId = 'selected_definition_set_id';
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
					"Minutes": 480,
					"End": scheduleDate + " 15:00",
				}
			],
			"IsFullDayAbsence": false,
			"DayOff": null
		};

		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(function () {
			fakeTeamsToggles = new FakeTeamsToggles();
			fakeTeamsPermissions = new FakeTeamsPermissions();
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {};

				});
				$provide.service('teamsToggles', function () {
					return fakeTeamsToggles;
				})
				$provide.service('teamsPermissions', function () {
					return fakeTeamsPermissions;
				});
				$provide.service('CurrentUserInfo',
					function () {
						return {
							CurrentUserInfo: function () {
								return { DefaultTimeZone: defaultUserTimeZone };
							}
						};
					});
			});
		});

		function FakeTeamsToggles() {
			this._toggles = {};
			this.set = function (toggles) {
				this._toggles = toggles;
			}
			this.all = function () {
				return this._toggles;
			};
		}

		function FakeTeamsPermissions() {
			this.all = function () {
				return {
					IsRemoveAbsenceAvailable: true,
					IsModifyScheduleAvailable: true,
					HasRemoveActivityPermission: true,
					HasMoveActivityPermission: true,
					HasRemoveOvertimePermission: true
				};
			};
		}

		beforeEach(inject(function (PersonSelection, ScheduleManagement, ActivityValidator) {
			personSelection = PersonSelection;
			scheduleMgmt = ScheduleManagement;
			target = ActivityValidator;
		}));

		it('should return invalide people when overtime activity input time is greater than 36h', function () {
			var localSchedule = JSON.parse(JSON.stringify(schedule));
			localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

			scheduleMgmt.resetSchedules([localSchedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
			var timeRange = {
				startTime: moment("2016-05-01 2:00"),
				endTime: moment("2016-05-08 2:00")
			};

			var result = target.validateInputForOvertime(scheduleMgmt, timeRange, definitionSetId, defaultUserTimeZone);

			expect(result.length).toEqual(1);
			expect(result[0].PersonId.indexOf('SomeoneElse') > -1).toEqual(true);
		});

		it('should return invalide people when overtime activity input start time is ealier than start of belongsToDate' +
			'\n\t\tin condition of the overtime activity overlaps agents belongsToDate\'s shifts', function () {
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

				scheduleMgmt.resetSchedules([localSchedule], moment(scheduleDate));
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
				var timeRange = {
					startTime: moment("2016-05-11 23:00"), //ensure this start time is earlier than 12:00AM of belongsToDate
					endTime: moment("2016-05-12 09:00")    //ensure this end time will make overtime overlapping with agent's belongsToDate shift
				};

				var result = target.validateInputForOvertime(scheduleMgmt, timeRange, definitionSetId, defaultUserTimeZone);

				expect(result.length).toEqual(1);
				expect(result[0].PersonId.indexOf('SomeoneElse') > -1).toEqual(true);
			});



		it('should return invalide people when overtime activity input start time is ealier than start of belongsToDate in agent\'s timezone ' +
			'\n\t\tin condition of the overtime activity overlaps agents belongsToDate\'s shifts', function () {
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

				localSchedule.Timezone.IanaId = "America/Chicago";  //UTC-6

				scheduleMgmt.resetSchedules([localSchedule], moment(scheduleDate));
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
				var timeRange = {
					startTime: moment("2016-05-12 10:00"), //ensure this start time is earlier than 12:00AM of belongsToDate in agent's timezone
					endTime: moment("2016-05-12 16:00")    //ensure this end time will make overtime overlapping with agent's belongsToDate shift
				};

				var result = target.validateInputForOvertime(scheduleMgmt, timeRange, definitionSetId, defaultUserTimeZone);

				expect(result.length).toEqual(1);
				expect(result[0].PersonId.indexOf('SomeoneElse') > -1).toEqual(true);
			});


		it('should NOT return invalide people when overtime activity input start time is later than start of belongsToDate in agent\'s timezone ' +
			'\n\t\t in condition of the overtime activity overlaps agents\' belongsToDate shifts', function () {
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

				localSchedule.Timezone.IanaId = "America/Chicago";  //UTC-6

				scheduleMgmt.resetSchedules([localSchedule], moment(scheduleDate));
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
				var timeRange = {
					startTime: moment("2016-05-12 15:00"),  //ensure this start time is later than 12:00AM of belongsToDate in agent's timezone
					endTime: moment("2016-05-12 16:10")     //ensure this end time will make overtime overlapping with agent's belongsToDate shift
				};

				var result = target.validateInputForOvertime(scheduleMgmt, timeRange, definitionSetId, defaultUserTimeZone);

				expect(result.length).toEqual(0);
			});

		it('should return invalid people when overtime activity input start time is out of  normalized date range',
			function () {
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];
				localSchedule.Timezone.IanaId = "America/Chicago";  //UTC-6
				scheduleMgmt.resetSchedules([localSchedule], moment(scheduleDate));
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Date = '2016-04-12';
				personSchedule.IsSelected = true;

				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
				var timeRange = {
					startTime: moment("2016-05-12 15:00"),  //ensure this start time is later than 12:00AM of belongsToDate in agent's timezone
					endTime: moment("2016-05-12 16:10")     //ensure this end time will make overtime overlapping with agent's belongsToDate shift
				};

				var result = target.validateInputForOvertime(scheduleMgmt, timeRange, definitionSetId, defaultUserTimeZone);

				expect(result.length).toEqual(1);

			});

		it('should return false when moving to time changes the schedule start date', function () {
			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
			var newStartMoment = moment("2016-05-13 2:00");

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, defaultUserTimeZone);

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

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

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

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(true);
		});

		it('should return false when moving activity in previous day out of shift', function () {
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

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
		});

		it('should return false when moving activity in next day out of shift', function () {
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

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(false);
		});

		it('should return true when moving activity in next day within shift', function () {
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

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(result).toEqual(true);
		});

		it('should return true when overtime is selected', function () {
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
						"Start": scheduleDate + " 06:00",
						"Minutes": 120,
						"IsOvertime": true
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
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0]);
			var newStartMoment = moment(scheduleDate + " 07:00");

			var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

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

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

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

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

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
						"Minutes": 600
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

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should invalidate shift-move if shift will conflict with shift of tomorrow after the move when there are shifts outside view range', function () {
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
						"Minutes": 600
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
						"Start": nextDay + " 08:00",
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

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

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

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should invalidate shift-move if it is full day absence', function () {
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
				"IsFullDayAbsence": true,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(scheduleDate + " 10:00");

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should invalidate shift-move if it is empty day', function () {
			var scheduleToday = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Timezone": {
					IanaId: "Asia/Hong_Kong"
				},
				"Projection": [
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var newStartMoment = moment(scheduleDate + " 10:00");

			target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

			expect(target.getInvalidPeople().length).toEqual(1);
		});

		it('should validate timeRange if it wont cause shift exceeding 36 hours', function () {
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
						"Start": scheduleDate + " 11:00",
						"End": scheduleDate + " 19:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var timeRange = {
				startTime: moment(scheduleDate + " 09:00"),
				endTime: moment(scheduleDate + " 10:00")
			}

			var invalidPeople = target.validateInputForOvertime(scheduleMgmt, timeRange, null, "Asia/Hong_Kong");

			expect(invalidPeople.length).toEqual(0);
		});

		it('should invalidate timeRange if it will cause shift exceeding 36 hours', function () {
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
						"Start": scheduleDate + " 23:00",
						"End": "2016-05-13 23:00",
						"Minutes": 1440
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			};

			scheduleMgmt.resetSchedules([scheduleToday], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var timeRange = {
				startTime: moment(scheduleDate + " 09:00"),
				endTime: moment(scheduleDate + " 10:00")
			}

			var invalidPeople = target.validateInputForOvertime(scheduleMgmt, timeRange, null, "Asia/Hong_Kong");

			expect(invalidPeople.length).toEqual(1);
		});

		it('should invalidate if multiplicator definition set is not configured for the agent', function () {
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
						"Start": scheduleDate + " 23:00",
						"End": "2016-05-13 23:00",
						"Minutes": 1440
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null,
				"MultiplicatorDefinitionSetIds": []
			};

			scheduleMgmt.resetSchedules([scheduleToday], moment(scheduleDate), "Asia/Hong_Kong");
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			var timeRange = {
				startTime: moment(scheduleDate + " 09:00"),
				endTime: moment(scheduleDate + " 10:00")
			}

			var invalidPeople = target.validateInputForOvertime(scheduleMgmt, timeRange, 'anything', "Asia/Hong_Kong");

			expect(invalidPeople.length).toEqual(1);
		});
	});
})();
