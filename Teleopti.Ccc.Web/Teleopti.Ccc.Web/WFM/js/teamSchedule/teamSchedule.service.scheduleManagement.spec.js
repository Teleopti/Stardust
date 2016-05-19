﻿"use strict";

describe("[ScheduleManagement Service Test]", function() {
	var target;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (ScheduleManagement) {
		target = ScheduleManagement;
	}));

	var scheduleDate = "2016-01-01";
	var scheduleDateMoment = moment(scheduleDate);
	var schedule1 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 07:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 08:00:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

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

	it("Should get latest shift start in given schedules", function() {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);

		expect(target.getLatestStartOfSelectedSchedule(scheduleDateMoment, [schedule1.PersonId,schedule2.PersonId])).toEqual(moment(schedule2.Projection[0].Start));
	});
});