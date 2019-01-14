(function () {
	'use strict';

	describe('teamschedule move activity validator tests: ', function () {
		var target, personSelection, scheduleMgmt, fakeTeamsPermissions;
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
					"StartInUtc": "2016-05-11 23:00",
					"Minutes": 480,
					"End": scheduleDate + " 15:00",
					"EndInUtc": scheduleDate + " 07:00"
				}
			],
			"IsFullDayAbsence": false,
			"DayOff": null
		};

		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(function () {
			fakeTeamsPermissions = new FakeTeamsPermissions();
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return {};

				});
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

		function commonTestsInDifferentLocale() {

			it('should return invalide people when overtime activity input time is greater than 36h', function () {
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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

					scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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

					scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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
					var localSchedule = angular.copy(schedule);
					localSchedule.MultiplicatorDefinitionSetIds = [definitionSetId];

					localSchedule.Timezone.IanaId = "America/Chicago";  //UTC-6

					scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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
					scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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
				var localSchedule = JSON.parse(JSON.stringify(schedule));
				scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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
				var localSchedule = {
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
							"StartInUtc": "2016-05-11 17:00",
							"Minutes": 480,
							"End": scheduleDate + " 10:00",
							"EndInUtc": scheduleDate + " 02:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"Start": scheduleDate + " 10:00",
							"End": scheduleDate + " 18:00",
							"StartInUtc": scheduleDate + " 02:00",
							"EndInUtc": scheduleDate + " 10:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				}
				scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
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
				var localSchedule = {
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
							"StartInUtc": previousDay + " 14:00",
							"EndInUtc": previousDay + " 17:00"
						}, {
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Short Break",
							"StartInUtc": previousDay + " 17:00",
							"EndInUtc": scheduleDate + " 18:00"
						}, {
							"ShiftLayerIds": ["layer3"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": scheduleDate + " 18:00",
							"EndInUtc": scheduleDate + " 20:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], previousDay, 'Asia/Hong_Kong');

				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[1].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[1], personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 00:00", 'Asia/Hong_Kong');

				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

				expect(result).toEqual(true);
			});

			it('should return false when moving activity in previous day out of shift', function () {
				var previousDay = "2016-05-11";
				var localSchedule = {
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
							"End": scheduleDate + " 01:00",
							"StartInUtc": previousDay + " 14:00",
							"Minutes": 180
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
				var newStartMoment = moment(scheduleDate + " 01:00");

				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

				expect(result).toEqual(false);
			});

			it('should return false when moving activity in next day out of shift', function () {
				var nextDay = "2016-05-13";
				var localSchedule = {
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
							"End": nextDay + " 04:00",
							"StartInUtc": scheduleDate + " 17:00",
							"Minutes": 180
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
				var newStartMoment = moment(scheduleDate + " 01:00");

				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

				expect(result).toEqual(false);
			});

			it('should return true when moving activity in next day within shift', function () {
				var nextDay = "2016-05-13";
				var localSchedule = {
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
							"StartInUtc": scheduleDate + " 17:00",
							"EndInUtc": scheduleDate + " 20:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": scheduleDate + " 20:00",
							"EndInUtc": scheduleDate + " 21:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);
				var newStartMoment = moment.tz(nextDay + " 02:00", "Asia/Hong_Kong");

				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

				expect(result).toEqual(true);
			});

			it('should return false unless moving activity not interset the overnight shift of previous day', function () {
				var todaySchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-03",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-03 09:00",
							"EndInUtc": "2019-01-03 15:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-03 15:00",
							"EndInUtc": "2019-01-03 17:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var previousSchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-02",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-02 22:00",
							"EndInUtc": "2019-01-03 06:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([todaySchedule, previousSchedule], "2019-01-03", "Europe/Berlin");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], personSchedule);

				var newStartMoment = moment.tz("2019-01-03 08:00", "Europe/Berlin");
				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeTruthy();

				newStartMoment = moment.tz("2019-01-03 05:00", "Europe/Berlin");
				result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeFalsy();
			});

			it('should return false unless moving activity not interset the shift of next day', function () {
				var todaySchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-03",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-03 08:00",
							"EndInUtc": "2019-01-03 15:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-03 15:00",
							"EndInUtc": "2019-01-03 17:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var nextSchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-04",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-04 06:00",
							"EndInUtc": "2019-01-04 15:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([todaySchedule, nextSchedule], "2019-01-03", "Europe/Berlin");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[1].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[1], personSchedule);

				var newStartMoment = moment.tz("2019-01-04 04:00", "Europe/Berlin");
				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeTruthy();

				newStartMoment = moment.tz("2019-01-04 07:00", "Europe/Berlin");
				result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeFalsy();
			});

			it('should return false when moving activity of yesterday is intersect the shift of today', function () {
				var todaySchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-03",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-03 09:00",
							"EndInUtc": "2019-01-03 15:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-03 15:00",
							"EndInUtc": "2019-01-03 17:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var previousSchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-02",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer3"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-02 22:00",
							"EndInUtc": "2019-01-03 04:00"
						},
						{
							"ShiftLayerIds": ["layer4"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-03 04:00",
							"EndInUtc": "2019-01-03 06:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([todaySchedule, previousSchedule], "2019-01-03", "Europe/Berlin");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[1].Projections[1].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[1].Projections[1], personSchedule);

				var newStartMoment = moment.tz("2019-01-03 10:00", "Europe/Berlin");
				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeFalsy();

				newStartMoment = moment.tz("2019-01-03 08:00", "Europe/Berlin");
				result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeFalsy();

				newStartMoment = moment.tz("2019-01-03 06:00", "Europe/Berlin");
				result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Europe/Berlin");
				expect(result).toBeTruthy();
			});

			it('should return false when change belong to date', function () {
				var todaySchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-10",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-10 16:00",
							"EndInUtc": "2019-01-10 17:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-10 17:00",
							"EndInUtc": "2019-01-10 21:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var previousSchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-09",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer3"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-09 21:00",
							"EndInUtc": "2019-01-10 05:00"
						}

					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([todaySchedule, previousSchedule], "2019-01-10", "Etc/Utc");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[1].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[1].Projections[0], personSchedule);

				var newStartMoment = moment.tz("2019-01-10 07:00", "Etc/Utc");
				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Etc/Utc");
				expect(result).toBeFalsy();
			});

			it('should return true when moving overnight shift of yesterday if one activity of yesterday start 12:00 AM', function () {
				var todaySchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-10",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer1"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-10 16:00",
							"EndInUtc": "2019-01-10 17:00"
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-10 17:00",
							"EndInUtc": "2019-01-10 21:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var previousSchedule = {
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": "2019-01-09",
					"Timezone": {
						IanaId: "Europe/Berlin"
					},
					"Projection": [
						{
							"ShiftLayerIds": ["layer3"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Email",
							"StartInUtc": "2019-01-09 21:00",
							"EndInUtc": "2019-01-10 00:00"
						},
						{
							"ShiftLayerIds": ["layer4"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": "2019-01-10 00:00",
							"EndInUtc": "2019-01-10 05:00"
						}

					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([todaySchedule, previousSchedule], "2019-01-10", "Etc/Utc");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[1].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[1].Projections[0], personSchedule);

				var newStartMoment = moment.tz("2019-01-10 01:00", "Etc/Utc");
				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Etc/Utc");
				expect(result).toBeTruthy();
			});

			it('should return true when overtime is selected', function () {
				var localSchedule = {
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
							"StartInUtc": "2016-05-11 22:00",
							"EndInUtc": "2016-05-12 00:00",
							"IsOvertime": true
						},
						{
							"ShiftLayerIds": ["layer2"],
							"ParentPersonAbsences": null,
							"Color": "#80FF80",
							"Description": "Phone",
							"StartInUtc": scheduleDate + " 02:00",
							"EndInUtc": scheduleDate + " 10:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				}
				scheduleMgmt.resetSchedules([localSchedule], scheduleDate);
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.Shifts[0].Projections[0].Selected = true;
				personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0]);
				var newStartMoment = moment(scheduleDate + " 07:00");

				var result = target.validateMoveToTime(scheduleMgmt, newStartMoment, "Asia/Hong_Kong");

				expect(result).toEqual(true);
			});

			it('should validate move to time when moving shift to time in current date', function () {
				var localSchedule = {
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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 08:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 02:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

				expect(target.getInvalidPeople().length).toEqual(0);
			});

			it('should validate move to time when moving shift to time in current date on timezone that is different from logon user timezone', function () {
				var nextDay = '2016-05-13';
				var localSchedule = {
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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 09:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				var nextDaySchedule = {
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
							"StartInUtc": nextDay + " 00:00",
							"EndInUtc": nextDay + " 09:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule, nextDaySchedule], scheduleDate, "America/Denver");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 08:00", "America/Denver");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

				expect(target.getInvalidPeople().length).toEqual(0);
			});

			it('should validate move to time when moving shift to time on next date', function () {
				var nextDay = "2016-05-13";
				var localSchedule = {
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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 08:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([localSchedule], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(nextDay + " 02:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 10:00"
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
							"StartInUtc": scheduleDate + " 21:00",
							"EndInUtc": nextDay + " 05:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday, scheduleNextDay], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 23:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 10:00"
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
							"StartInUtc": nextDay + " 00:00",
							"EndInUtc": nextDay + " 08:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday, scheduleNextDay], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 23:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

				expect(target.getInvalidPeople().length).toEqual(1);
			});

			it('should invalidate shift-move if shift will conflict with shift of yesterday after the move', function () {
				var prevDay = "2016-05-11";
				var scheduleToday = {
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
							"StartInUtc": scheduleDate + " 05:00",
							"EndInUtc": scheduleDate + " 13:00"
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
							"StartInUtc": prevDay + " 15:00",
							"EndInUtc": prevDay + " 23:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday, scheduleNextDay], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 05:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

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
							"StartInUtc": scheduleDate + " 00:00",
							"EndInUtc": scheduleDate + " 08:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": true,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 10:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

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

				scheduleMgmt.resetSchedules([scheduleToday], scheduleDate, "Asia/Hong_Kong");
				var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				var newStartMoment = moment.tz(scheduleDate + " 10:00", "Asia/Hong_Kong");

				target.validateMoveToTimeForShift(scheduleMgmt, newStartMoment);

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
							"StartInUtc": scheduleDate + " 03:00",
							"EndInUtc": scheduleDate + " 11:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday], scheduleDate, "Asia/Hong_Kong");
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
							"StartInUtc": scheduleDate + " 15:00",
							"EndInUtc": "2016-05-14 15:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				};

				scheduleMgmt.resetSchedules([scheduleToday], scheduleDate, "Asia/Hong_Kong");
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
							"StartInUtc": scheduleDate + " 15:00",
							"StartInUtc": "2016-05-14 15:00"
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null,
					"MultiplicatorDefinitionSetIds": []
				};

				scheduleMgmt.resetSchedules([scheduleToday], scheduleDate, "Asia/Hong_Kong");
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
		}

		commonTestsInDifferentLocale();

		describe('in locale ar-AE', function () {
			beforeAll(function () {
				moment.locale('ar-AE');
			});

			afterAll(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});

		describe('in locale fa-IR', function () {
			beforeAll(function () {
				moment.locale('fa-IR');
			});

			afterAll(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});
	});
})();
