describe("teamschedule schedule management service tests", function() {
	"use strict";
	var target, fakeTeamScheduleSvc;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});


	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(function () {
		fakeTeamScheduleSvc = new FakeTeamScheduleSvc();
		module(function ($provide) {
			$provide.service('TeamSchedule', function() {
				return fakeTeamScheduleSvc;
			});
		});
	});

	beforeEach(inject(function (ScheduleManagement) {
		target = ScheduleManagement;
	}));


	function FakeTeamScheduleSvc() {
		var newData = {
			Schedules: [
				{
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": scheduleDate,
					"Projection": [
						{
							"Color": "#80FF80",
							"Description": "Phone",
							"Start": scheduleDate + " 11:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				},
				{
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": yesterday,
					"Projection": [
						{
							"Color": "#80FF80",
							"Description": "Email",
							"Start": yesterday + " 21:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				}
			]
		};
		this.getSchedules = function (date, agents) {
			return {
				then: function(cb) {
					cb(newData);
				}
			}
		}
	}

	var scheduleDate = "2016-01-02";
	var yesterday = "2016-01-01";
	var scheduleDateMoment = moment(scheduleDate);
	var schedule1 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 11:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null,
		"UnderlyingScheduleSummary": {
			"PersonalActivities": [{
				"Description": "personal activity",
				"Start": scheduleDate + ' 10:00',
				"End": scheduleDate + ' 11:00'
			}],
			"PersonPartTimeAbsences": [{
				"Description": "holiday",
				"Start": scheduleDate + ' 11:30',
				"End": scheduleDate + ' 12:00'
			}],
			"PersonMeetings": [{
				"Description": "administration",
				"Start": scheduleDate + ' 14:00',
				"End": scheduleDate + ' 15:00'
			}]
		}
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 12:00:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

	var schedule3 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": yesterday,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": yesterday + " 21:00",
				"Minutes": 480
			}

		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

	it("Can recreate schedule view model with specified timezone", function() {
		target.resetSchedules([schedule1], scheduleDateMoment);

		var timezone = 'Europe/Berlin';

		target.recreateScheduleVm(scheduleDateMoment, timezone);
		var schedule = target.groupScheduleVm.Schedules[0];

		expect(schedule.Shifts[0].Projections[0].Start).toEqual(scheduleDate + "T04:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].Start).toEqual(scheduleDate + "T03:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].End).toEqual(scheduleDate + "T04:00");
		expect(schedule.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].Start).toEqual(scheduleDate + "T04:30");
		expect(schedule.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].End).toEqual(scheduleDate + "T05:00");
		expect(schedule.UnderlyingScheduleSummary.PersonMeetings[0].Start).toEqual(scheduleDate + "T07:00");
		expect(schedule.UnderlyingScheduleSummary.PersonMeetings[0].End).toEqual(scheduleDate + "T08:00");
	});
	

	it("Can create group schedule", inject(function () {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		var schedules = target.groupScheduleVm.Schedules;
		expect(schedules.length).toEqual(2);
	}));

	it("Should reset group schedule", inject(function () {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);

		target.resetSchedules([schedule1], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(1);
	}));

	it("Should update group schedule and keep the order", inject(function () {
		target.resetSchedules([schedule1, schedule3, schedule2], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);

		target.updateScheduleForPeoples(["221B-Baker-SomeoneElse"], scheduleDateMoment,null, function (){});
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);
		expect(target.rawSchedules[0].PersonId).toEqual("221B-Baker-SomeoneElse");
		expect(target.rawSchedules[0].Date).toEqual(scheduleDate);
		expect(target.rawSchedules[0].Projection[0].Description).toEqual("Phone");
	}));

});