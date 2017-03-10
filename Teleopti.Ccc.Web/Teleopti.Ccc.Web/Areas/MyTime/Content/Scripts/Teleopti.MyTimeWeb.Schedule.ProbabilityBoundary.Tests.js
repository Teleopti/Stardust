/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

	var yearAndMonth = "2017-02-";
	var dayForToday = 23;
	var yesterdayForBaseDate = yearAndMonth + (dayForToday - 1);
	var baseDate = yearAndMonth + dayForToday;
	var tomorrowForBaseDate = yearAndMonth + (dayForToday + 1);

	var createPeriods = function () {
		return [
			{
				"StartTime": baseDate + "T09:30:00",
				"EndTime": baseDate + "T18:30:00"
			}
		];
	}

	var createCrossDayPeriodsEndAtToday = function () {
		return [
			{
				"StartTime": yesterdayForBaseDate + "T17:00:00",
				"EndTime": baseDate + "T01:00:00"
			}
		];
	}

	var createCrossDayPeriodsEndAtTomorrow = function () {
		return [
			{
				"StartTime": baseDate + "T17:00:00",
				"EndTime": tomorrowForBaseDate + "T01:00:00"
			}
		];
	}

	var createScheduleDay = function (isFullDayAbsence, isDayOff, periods) {
		return {
			FixedDate: baseDate,
			IsFullDayAbsence: isFullDayAbsence,
			IsDayOff: isDayOff,
			Periods: periods
		};
	}

	var createTimelines = function (timelineStartHour, timelineEndHour) {
		var timelinePoints = [];
		var startHour = timelineStartHour;
		var endHour = timelineEndHour;

		if (startHour > 0) {
			timelinePoints.push({
				"minutes": startHour * 60 - constants.intervalLengthInMinutes,
				"timeText": (startHour - 1) + ":45"
			});
		}

		for (var i = startHour; i <= endHour; i++) {
			timelinePoints.push({
				"minutes": i * 60,
				"timeText": i + ":00"
			});
		}

		if (endHour < 24) {
			timelinePoints.push({
				"minutes": endHour * 60 + constants.intervalLengthInMinutes,
				"timeText": endHour + ":15"
			});
		}

		return timelinePoints;
	}

	test("Calculate absence probability boundaries for dayoff", function () {
		var scheduleDay = createScheduleDay(false, true, []);
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.absenceProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate absence probability boundaries for full day absence", function () {
		var scheduleDay = createScheduleDay(true, false, []);
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.absenceProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate overtime probability boundaries for dayoff", function () {
		var scheduleDay = createScheduleDay(false, true, []);
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.overtimeProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate overtime probability boundaries for full day absence", function () {
		var scheduleDay = createScheduleDay(true, false, []);
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.overtimeProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate absence probability boundaries for normal day", function () {
		var scheduleDay = createScheduleDay(false, false, createPeriods());
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.absenceProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1110);
		equal(Math.round(vm.probabilityStartPosition * 1000), Math.round(570 * 1000 / constants.totalMinutesOfOneDay));
		equal(Math.round(vm.probabilityEndPosition * 1000), Math.round(1110 * 1000 / constants.totalMinutesOfOneDay));
	});

	// Should show overtime probability in intersection of timeline, open hour period and probility start / end
	test("Calculate overtime probability boundaries for normal day", function () {
		var scheduleDay = createScheduleDay(false, false, createPeriods());

		var timelineStartHour = 6;
		var timelineEndHour = 19;
		var timelines = createTimelines(timelineStartHour, timelineEndHour);

		var timelineStartInMinutes = timelineStartHour * 60 - constants.intervalLengthInMinutes;
		var timelineEndInMinutes = timelineEndHour * 60 + constants.intervalLengthInMinutes;
		var timelineLengthInMinutes = timelineEndInMinutes - timelineStartInMinutes;

		var openHourPeriod = {
			startTime: "07:00:00",
			endTime: "16:00:00"
		};
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.overtimeProbabilityType, [], openHourPeriod);

		equal(vm.lengthPercentagePerMinute, 1 / timelineLengthInMinutes);
		equal(vm.probabilityStartMinutes, 420); // 07:00
		equal(vm.probabilityEndMinutes, 960); // 16:00
		equal(Math.round(vm.probabilityStartPosition * 1000),
			Math.round(((420 - timelineStartInMinutes) * vm.lengthPercentagePerMinute) * 1000));
		equal(Math.round(vm.probabilityEndPosition * 1000),
			Math.round(((960 - timelineStartInMinutes) * vm.lengthPercentagePerMinute) * 1000));
	});

	test("Calculate absence probability boundaries for cross day schedule end today", function () {
		var scheduleDay = createScheduleDay(false, false, createCrossDayPeriodsEndAtToday());
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.absenceProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, 60);
		equal(vm.probabilityStartPosition, 0);
		equal(Math.round(vm.probabilityEndPosition * 1000), Math.round(60 * 1000 / constants.totalMinutesOfOneDay));
	});

	test("Calculate absence probability boundaries for cross day schedule will end tomorrow", function () {
		var scheduleDay = createScheduleDay(false, false, createCrossDayPeriodsEndAtTomorrow());
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.absenceProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 1020);
		equal(vm.probabilityEndMinutes, 1440);
		equal(Math.round(vm.probabilityStartPosition * 1000), Math.round(1020 * 1000 / constants.totalMinutesOfOneDay));
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate overtime probability boundaries for cross day schedule end today", function () {
		var scheduleDay = createScheduleDay(false, false, createCrossDayPeriodsEndAtToday());
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.overtimeProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, 1440);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});

	test("Calculate overtime probability boundaries for cross day schedule will end tomorrow", function () {
		var scheduleDay = createScheduleDay(false, false, createCrossDayPeriodsEndAtTomorrow());
		var timelines = createTimelines(0, 24);
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, timelines,
			constants.overtimeProbabilityType, [], undefined);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, 1440);
		equal(vm.probabilityStartPosition, 0);
		equal(vm.probabilityEndPosition, 1);
	});
});