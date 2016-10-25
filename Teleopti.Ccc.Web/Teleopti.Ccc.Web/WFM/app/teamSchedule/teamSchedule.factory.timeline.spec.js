﻿"use strict";

describe("teamschedule timeLine tests", function () {

	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";
	var mockCurrentUserInfo = {
		DefaultTimeZone: "Etc/UTC",
		DateFormatLocale: "en-GB"
	};

	beforeEach(function() {
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});

		moment.locale(mockCurrentUserInfo.DateFormatLocale);
	});
	

	it("should display 1 extra hour line when schedule starts or end at hour point", inject(function (TeamScheduleTimeLineFactory) {

		var target = TeamScheduleTimeLineFactory;

		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");
		var now = moment(today + " 07:35:00");

		var schedules = [
			createSchedule(today, null, [{ startHour: 12, endHour: 16 }, { startHour: 16, endHour: 20 }]),
			createSchedule(today, null, [{ startHour: 8, endHour: 10 }, { startHour: 10, endHour: 16 }]),
			createSchedule(tomorrow, 'dayOff')
		];

		var timeLine = target.Create(schedules, now);
		
		var expectedStart = 420;
		var expectedEnd = 1260;
		var expectedHourPointsCount = 15;

		expect(timeLine.Offset.format(dateTimeFormat)).toEqual(now.startOf("day").format(dateTimeFormat));
		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
		expect(timeLine.LengthPercentPerMinute).toEqual(100 / (expectedEnd - expectedStart));

		expect(timeLine.HourPoints.length).toEqual(expectedHourPointsCount);

		var firstHourPoint = timeLine.HourPoints[0];
		expect(firstHourPoint.TimeLabel).toEqual("07:00");
		expect(firstHourPoint.Position()).toEqual(0);

		var lastHourPoint = timeLine.HourPoints[expectedHourPointsCount - 1];
		expect(lastHourPoint.TimeLabel).toEqual("21:00");
		expect(lastHourPoint.Position()).toEqual(100);
	}));

	it("should display same whole hour line as the start or end time when schedule starts or ends not at hour point", inject(function (TeamScheduleTimeLineFactory) {

		var target = TeamScheduleTimeLineFactory;

		var today = "2015-10-26";

		var now = moment(today + " 07:35:00");

		var schedules = [
			createSchedule(today, null, [{ startHour: 12, endHour: 16 }, { startHour: 16, endHour: 20.5 }]),
			createSchedule(today, null, [{ startHour: 7.5, endHour: 9.5 }, { startHour: 10, endHour: 16 }])
		];

		var timeLine = target.Create(schedules, now);
		
		var expectedStart = 420;
		var expectedEnd = 1260;
		var expectedHourPointsCount = 15;

		expect(timeLine.Offset.format(dateTimeFormat)).toEqual(now.startOf("day").format(dateTimeFormat));
		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
		expect(timeLine.LengthPercentPerMinute).toEqual(100 / (expectedEnd - expectedStart));

		expect(timeLine.HourPoints.length).toEqual(expectedHourPointsCount);

		var firstHourPoint = timeLine.HourPoints[0];
		expect(firstHourPoint.TimeLabel).toEqual("07:00");
		expect(firstHourPoint.Position()).toEqual(0);

		var lastHourPoint = timeLine.HourPoints[expectedHourPointsCount - 1];
		expect(lastHourPoint.TimeLabel).toEqual("21:00");
		expect(lastHourPoint.Position()).toEqual(100);
	}));

	it("should display end hour point within maximum view range for today's schedules", inject(function(TeamScheduleTimeLineFactory) {
		var target = TeamScheduleTimeLineFactory;

		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");

		var now = moment(today + " 07:35:00");

		var maxViewRange = {
			startMoment: moment(today).startOf('day'),
			endMoment: moment(today).startOf('day').add(30, 'hour')
		};

		var schedules = [
			createSchedule(today, null, [{ startHour: 12, endHour: 16 }, { startHour: 16, endHour: 20.5 }]),
			createSchedule(today, null, [{ startHour: 7.5, endHour: 9.5 }, { startHour: 12, endHour: 31 }])
		];

		var timeLine = target.Create(schedules, now, maxViewRange);

		var expectedStart = 420;
		var expectedEnd = 1800;

		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
	}));

	it("should display end hour point within maximum view range with tomorrow's schedules", inject(function (TeamScheduleTimeLineFactory) {
		var target = TeamScheduleTimeLineFactory;

		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");

		var now = moment(today + " 07:35:00");

		var maxViewRange = {
			startMoment: moment(today).startOf('day'),
			endMoment: moment(today).startOf('day').add(30, 'hour')
		};

		var schedules = [
			createSchedule(today, null, [{ startHour: 12, endHour: 16 }, { startHour: 16, endHour: 20.5 }]),
			createSchedule(tomorrow, null, [{ startHour: 4, endHour: 7 }, { startHour: 7, endHour: 11 }])
		];

		var timeLine = target.Create(schedules, now, maxViewRange);

		var expectedStart = 660;
		var expectedEnd = 1800;

		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
	}));

	it("should display end hour point within maximum view range with only tomorrow's schedules", inject(function (TeamScheduleTimeLineFactory) {
		var target = TeamScheduleTimeLineFactory;

		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");

		var now = moment(today + " 07:35:00");

		var maxViewRange = {
			startMoment: moment(today).startOf('day'),
			endMoment: moment(today).startOf('day').add(30, 'hour')
		};

		var schedules = [
			createSchedule(tomorrow, null, [{ startHour: 4, endHour: 7 }, { startHour: 7, endHour: 11 }])
		];

		var timeLine = target.Create(schedules, now, maxViewRange);

		var expectedStart = 480;
		var expectedEnd = 1800;

		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
	}));


	function createSchedule(belongsToDate, dayOff, projectionInfoArray) {
		var dateMoment = moment(belongsToDate);
		var projections = [];

		var fakeSchedule = {
			Date: dateMoment.format('YYYY-MM-DD'),
			DayOff: dayOff == null ? null : createDayOff(),
			Projection: createProjection()
		};

		function createProjection() {

			if (dayOff == null) {
				projectionInfoArray.forEach(function (projectionInfo) {

					projections.push({
						Start: moment(dateMoment).add(projectionInfo.startHour, 'hours').format('YYYY-MM-DD HH:mm'),
						End: moment(dateMoment).add(projectionInfo.endHour, 'hours').format('YYYY-MM-DD HH:mm'),
						Minutes: moment.duration(projectionInfo.endHour - projectionInfo.startHour, 'hours').asMinutes()
					});
				});
			}

			return projections;
		};

		function createDayOff() {
			return {
				DayOffName: 'Day off',
				Start: dateMoment.format('YYYY-MM-DD HH:mm'),
				Minutes: 1440
			};

		};

		return fakeSchedule;
	}
});
