/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.Helper");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityCssClass = "probability-none";
	var expiredProbabilityCssClass = "probability-expired";

	var yesterday = "2017-03-09";
	var baseDate = "2017-03-10";
	var tomorrow = "2017-03-11";

	var createTimeline = function (timelineStartHour, timelineEndHour) {
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

	var createRawProbabilities = function () {
		var result = [];
		for (var i = 0; i < 24 * 60 / 15; i++) {
			result.push({
				"StartTime": moment(baseDate).add(15 * i, "minutes").toDate(),
				"EndTime": moment(baseDate).add(15 * (i + 1), "minutes").toDate(),
				"Possibility": Math.round(Math.random())
			});
		}

		return result;
	}

	var createProbabilities = function (layoutDirection) {
		var scheduleDay = {
			FixedDate: baseDate,
			IsFullDayAbsence: false,
			IsDayOff: false,
			Periods: [
				{
					"StartTime": baseDate + "T02:45:00",
					"EndTime": baseDate + "T04:00:00"
				}, {
					"StartTime": baseDate + "T04:00:00",
					"EndTime": baseDate + "T04:15:00"
				}, {
					"StartTime": baseDate + "T04:15:00",
					"EndTime": baseDate + "T06:00:00"
				}
			]
		};

		var rawProbability = createRawProbabilities();
		var options = {
			staffingProbabilityEnabled: true,
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: layoutDirection,
			timelines: createTimeline(1, 8),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		return Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
	}

	test("No continous period for empty schedule periods", function () {
		var schedulePeriods = [];
		var vm = new Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(baseDate, schedulePeriods);

		equal(vm.length, 0);
	});

	test("Should get one continous periods", function () {
		var schedulePeriods = [
			{
				"StartTime": baseDate + "T02:45:00",
				"EndTime": baseDate + "T04:00:00"
			}, {
				"StartTime": baseDate + "T04:00:00",
				"EndTime": baseDate + "T04:15:00"
			}, {
				"StartTime": baseDate + "T04:15:00",
				"EndTime": baseDate + "T06:00:00"
			}
		];

		var vm = new Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(baseDate, schedulePeriods);
		equal(vm.length, 1);
		equal(vm[0].startTime, 165);
		equal(vm[0].endTime, 360);
	});

	test("Should get correct continous periods", function () {
		var schedulePeriods = [
			{
				"StartTime": baseDate + "T02:45:00",
				"EndTime": baseDate + "T04:00:00"
			}, {
				"StartTime": baseDate + "T04:00:00",
				"EndTime": baseDate + "T04:15:00"
			}, {
				"StartTime": baseDate + "T06:00:00",
				"EndTime": baseDate + "T09:00:00"
			}
		];

		var vm = new Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(baseDate, schedulePeriods);
		equal(vm.length, 2);

		var firstContinousPeriod = vm[0];
		equal(firstContinousPeriod.startTime, 165);
		equal(firstContinousPeriod.endTime, 255);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTime, 360);
		equal(secondContinousPeriod.endTime, 540);
	});

	test("Should get correct starttime and endtime for cross day schedules", function () {
		var schedulePeriods = [
			{
				"StartTime": yesterday + "T22:45:00",
				"EndTime": baseDate + "T02:00:00"
			}, {
				"StartTime": baseDate + "T02:00:00",
				"EndTime": baseDate + "T04:00:00"
			}, {
				"StartTime": baseDate + "T17:15:00",
				"EndTime": tomorrow + "T01:30:00"
			}
		];

		var vm = new Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(baseDate, schedulePeriods);
		equal(vm.length, 2);

		var firstContinousPeriod = vm[0];
		equal(firstContinousPeriod.startTime, 0);
		equal(firstContinousPeriod.endTime, 240);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTime, 1035);
		equal(secondContinousPeriod.endTime, 1440);
	});

	test("Should not create probability if feature is disabled", function () {
		var scheduleDay = {};
		var rawProbability = createRawProbabilities();
		var options = { staffingProbabilityEnabled: false };

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should not create probability if set to show no probability", function () {
		var scheduleDay = {};
		var rawProbability = createRawProbabilities();
		var options = {
			staffingProbabilityEnabled: true,
			probabilityType: constants.noneProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should not create probability if set to show absence probability but is full day absence", function () {
		var scheduleDay = {
			IsFullDayAbsence: true,
			IsDayOff: false
		};
		var rawProbability = createRawProbabilities();
		var options = {
			staffingProbabilityEnabled: true,
			probabilityType: constants.absenceProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should not create probability if set to show absence probability but is dayoff", function () {
		var scheduleDay = {
			IsFullDayAbsence: false,
			IsDayOff: true
		};
		var rawProbability = createRawProbabilities();
		var options = {
			staffingProbabilityEnabled: true,
			probabilityType: constants.absenceProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should create probability with height for vertical layout direction", function () {
		var probabilities = createProbabilities(constants.verticalDirectionLayout)
		equal(probabilities.length, 14);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.height != undefined, true)
			equal(probabilities[i].styleJson.width == undefined, true)
		}
	});

	test("Should create probability with height for horizontal layout direction", function () {
		var probabilities = createProbabilities(constants.horizontalDirectionLayout)
		equal(probabilities.length, 14);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.width != undefined, true)
			equal(probabilities[i].styleJson.height == undefined, true)
		}
	});
});