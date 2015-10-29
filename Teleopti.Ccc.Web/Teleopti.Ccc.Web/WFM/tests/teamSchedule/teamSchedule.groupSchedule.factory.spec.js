"use strict";

describe("GroupScheduleFactory", function () {
	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";

	var target;

	beforeEach(module("wfm.teamSchedule"));

	// Setup the mock service in an anonymous module.
	beforeEach(function () {
		module('currentUserInfoService');
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					DefaultTimeZone: "Etc/UTC",
					DateFormatLocale: "en-GB"
				};
			});
		});
	});

	//Getting reference of the mocked service
	var mockCurrentUserInfo;
	beforeEach(inject(function (CurrentUserInfo) {
		mockCurrentUserInfo = CurrentUserInfo;
		moment.locale(mockCurrentUserInfo.DateFormatLocale);
	}));

	beforeEach(inject(function (GroupScheduleFactory) {
		target = GroupScheduleFactory;
	}));

	it("Schdule Sorting - Schedule sort by start time", inject(function () {
		var today = "2015-10-26";
		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": today,
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"Start": today + " 08:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			},
			{
				"PersonId": "221B-Baker-Watson",
				"Name": "Dr. Watson",
				"Date": today,
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"Start": today + " 07:30",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			},
			{
				"PersonId": "221B-Sherlock",
				"Name": "Sherlock Holmes",
				"Date": today,
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"Start": today + " 08:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
		];

		var groupScheduleVm = target.Create(schedules, now);

		var timeLine = groupScheduleVm.TimeLine;
		expect(timeLine).toBeDefined();

		var personSchedules = groupScheduleVm.Schedules;
		expect(personSchedules.length).toEqual(3);
		expect(personSchedules[0].Name).toEqual("Dr. Watson");
		expect(personSchedules[1].Name).toEqual("Sherlock Holmes");
		expect(personSchedules[2].Name).toEqual("SomeoneElse");
	}));

	it("Schdule Sorting - Absence should before shift", inject(function () {
		var today = "2015-10-26";
		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"PersonId": "221B-Sherlock",
				"Name": "Sherlock Holmes",
				"Date": today,
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"Start": today + " 07:30",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			},
			{
				"PersonId": "221B-Baker-Watson",
				"Name": "Dr. Watson",
				"Date": today,
				"Projection": [
				   {
				   	"Color": "#1E90FF",
				   	"Description": "AWOL",
				   	"Start": today + " 07:00",
				   	"Minutes": 480
				   }
				],
				"IsFullDayAbsence": true,
				"DayOff": null
			}
		];

		var groupScheduleVm = target.Create(schedules, now);

		var timeLine = groupScheduleVm.TimeLine;
		expect(timeLine).toBeDefined();

		var personSchedules = groupScheduleVm.Schedules;
		expect(personSchedules.length).toEqual(2);
		expect(personSchedules[0].Name).toEqual("Dr. Watson");
		expect(personSchedules[1].Name).toEqual("Sherlock Holmes");
	}));

	it("Schdule Sorting - Shift should before dayoff", inject(function () {
		var today = "2015-10-26";
		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"PersonId": "221B-Sherlock",
				"Name": "Sherlock Holmes",
				"Date": today,
				"Projection": [],
				"IsFullDayAbsence": false,
				"DayOff":
				{
					"DayOffName": "Day off",
					"Start": "2015-10-26 23:00",
					"Minutes": 1440
				}
			},
			{
				"PersonId": "221B-Baker-Watson",
				"Name": "Dr. Watson",
				"Date": today,
				"Projection": [
				   {
				   	"Color": "#1E90FF",
				   	"Description": "Email",
				   	"Start": today + " 07:00",
				   	"Minutes": 480
				   }
				],
				"IsFullDayAbsence": true,
				"DayOff": null
			}
		];

		var groupScheduleVm = target.Create(schedules, now);

		var timeLine = groupScheduleVm.TimeLine;
		expect(timeLine).toBeDefined();

		var personSchedules = groupScheduleVm.Schedules;
		expect(personSchedules.length).toEqual(2);
		expect(personSchedules[0].Name).toEqual("Dr. Watson");
		expect(personSchedules[1].Name).toEqual("Sherlock Holmes");
	}));

	it("Schdule Sorting - Dayoff should before empty schedule", inject(function () {
		var today = "2015-10-26";
		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"PersonId": "221B-Sherlock",
				"Name": "Sherlock Holmes",
				"Date": today,
				"Projection": [],
				"IsFullDayAbsence": false,
				"DayOff":
				{
					"DayOffName": "Day off",
					"Start": "2015-10-26 23:00",
					"Minutes": 1440
				}
			},
			{
				"PersonId": "221B-Baker-Watson",
				"Name": "Dr. Watson",
				"Date": today,
				"Projection": [],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
		];

		var groupScheduleVm = target.Create(schedules, now);

		var timeLine = groupScheduleVm.TimeLine;
		expect(timeLine).toBeDefined();

		var personSchedules = groupScheduleVm.Schedules;
		expect(personSchedules.length).toEqual(2);
		expect(personSchedules[0].Name).toEqual("Dr. Watson");
		expect(personSchedules[1].Name).toEqual("Sherlock Holmes");
	}));
});