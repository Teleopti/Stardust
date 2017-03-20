/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.ProbabilityModels");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var yesterday = "2017-03-09";
	var baseDate = "2017-03-10";
	var tomorrow = "2017-03-11";

	function createTimeline(timelineStartHour, timelineEndHour) {
		var timelinePoints = [];
		var startHour = timelineStartHour;
		var endHour = timelineEndHour;

		if (startHour > 0) {
			timelinePoints.push({
				"minutes": startHour * 60 - constants.timelineMarginInMinutes,
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
				"minutes": endHour * 60 + constants.timelineMarginInMinutes,
				"timeText": endHour + ":15"
			});
		}

		return timelinePoints;
	}

	function createRawProbabilities() {
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
		equal(vm[0].startTimeInMin, 165);
		equal(vm[0].endTimeInMin, 360);
	});

	test("Should get multiple continous periods - 1", function () {
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
		equal(firstContinousPeriod.startTimeInMin, 165);
		equal(firstContinousPeriod.endTimeInMin, 255);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTimeInMin, 360);
		equal(secondContinousPeriod.endTimeInMin, 540);
	});

	test("Should get multiple continous periods - 2", function () {
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
			}, {
				"StartTime": baseDate + "T14:00:00",
				"EndTime": baseDate + "T20:00:00"
			}
		];

		var vm = new Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(baseDate, schedulePeriods);
		equal(vm.length, 3);

		var firstContinousPeriod = vm[0];
		equal(firstContinousPeriod.startTimeInMin, 165);
		equal(firstContinousPeriod.endTimeInMin, 255);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTimeInMin, 360);
		equal(secondContinousPeriod.endTimeInMin, 540);

		var thirdContinousPeriod = vm[2];
		equal(thirdContinousPeriod.startTimeInMin, 840);
		equal(thirdContinousPeriod.endTimeInMin, 1200);
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
		equal(firstContinousPeriod.startTimeInMin, 0);
		equal(firstContinousPeriod.endTimeInMin, 240);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTimeInMin, 1035);
		equal(secondContinousPeriod.endTimeInMin, 1440);
	});

	test("Should not create probability if set to show no probability", function () {
		var scheduleDay = {};
		var rawProbability = createRawProbabilities();
		var options = {
			probabilityType: constants.noneProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should not create probability if set to show absence probability but is full day absence", function () {
		var scheduleDay = {
			IsFullDayAbsence: true,
			IsDayOff: false
		};
		var rawProbability = createRawProbabilities();
		var options = {
			probabilityType: constants.absenceProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should not create probability if set to show absence probability but is dayoff", function () {
		var scheduleDay = {
			IsFullDayAbsence: false,
			IsDayOff: true
		};
		var rawProbability = createRawProbabilities();
		var options = {
			probabilityType: constants.absenceProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 0);
	});

	test("Should create probability with height for vertical layout direction", function () {
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
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: constants.verticalDirectionLayout,
			mergeSameIntervals: false,
			timelines: createTimeline(1, 8),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);

		equal(probabilities.length, 13);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.height != undefined, true);
			equal(probabilities[i].styleJson.width == undefined, true);
		}
	});

	test("Should create probability with height for horizontal layout direction", function () {
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
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: constants.horizontalDirectionLayout,
			mergeSameIntervals: false,
			timelines: createTimeline(1, 8),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 13);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.width != undefined, true);
			equal(probabilities[i].styleJson.height == undefined, true);
		}
	});

	test("Should merge same probability intervals when options.mergeSameIntervals is set to true", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var timelineStart = 1, timelineEnd = 9;
		var scheduleDay = {
			FixedDate: baseDate,
			IsFullDayAbsence: false,
			IsDayOff: false,
			Periods: [
	 			{
	 				"StartTime": baseDate + "T06:00:00",
	 				"EndTime": baseDate + "T09:00:00"
	 			}
			]
		};
		var rawProbabilities = createRawProbabilities();

		rawProbabilities.forEach(function (p, index) {
			//Test case: probability level is low from 6:00 to 7:00, and high between 7:00 to 9:00
			//Schedule Period: 6:00 ~ 9:00, Timeline: 0:45 ~ 9:15
			p.Possibility = index < (7 * 4) ? constants.probabilityLow : constants.probabilityHigh;
		});

		var expectedRawProbabilities = rawProbabilities.filter(function (p) {
			return moment(p.EndTime) <= moment(baseDate).add(9, "hours");
		});

		var options = {
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: constants.horizontalDirectionLayout,
			mergeSameIntervals: true,
			timelines: createTimeline(timelineStart, timelineEnd),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay,
			expectedRawProbabilities,
			{
				userNowInMinute: function () { return 0; }
			}, options);

		var expectedLengthPercentagePerMinute = 1 / ((timelineEnd - timelineStart) * 60 + 2 * constants.timelineMarginInMinutes);

		equal(probabilities.length, 2);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.width != undefined, true);
			equal(probabilities[i].styleJson.height == undefined, true);
		}

		// Should calculate the margin on top for this invisible probability view model
		var probability = probabilities[0];
		//equal(probability.styleJson.left,
		//	((((7 - timelineStart) * 60 - constants.timelineMarginInMinutes) * expectedLengthPercentagePerMinute * 100) + "%"));
		//equal(probability.styleJson.width,
		//	((((7 - 6) * 60) * expectedLengthPercentagePerMinute * 100) + "%"));
		equal(probability.tooltips().indexOf("6:00 - 7:00") > -1, true);
		equal(probability.cssClass(), "probability-low");

		probability = probabilities[1];
		//equal(probability.styleJson.width, (((9 - 7) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
		equal(probability.tooltips().indexOf("7:00 - 9:00") > -1, true);
		equal(probability.cssClass(), "probability-high");
	});

	test("Should merge same probability intervals correctly when there are cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var timelineStart = 0, timelineEnd = 9;
		var scheduleDay = {
			FixedDate: baseDate,
			IsFullDayAbsence: false,
			IsDayOff: false,
			Periods: [
	 			{
	 				"StartTime": baseDate + "T00:00:00",
	 				"EndTime": baseDate + "T04:00:00"
	 			},
	 			{
	 				"StartTime": baseDate + "T06:00:00",
	 				"EndTime": baseDate + "T09:00:00"
	 			}
			]
		};
		var rawProbabilities = createRawProbabilities();

		rawProbabilities.forEach(function (p, index) {
			//Test case: probability level is low from 6:00 to 7:00, and high between 7:00 to 9:00
			//Schedule Period: 6:00 ~ 9:00, Timeline: 0:45 ~ 9:15
			p.Possibility = index < (7 * 4) ? constants.probabilityLow : constants.probabilityHigh;
		});

		var expectedRawProbabilities = rawProbabilities.filter(function (p) {
			return moment(p.EndTime) <= moment(baseDate).add(9, "hours");
		});

		var options = {
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: constants.horizontalDirectionLayout,
			mergeSameIntervals: true,
			hideProbabilityEarlierThanNow: false,
			timelines: createTimeline(timelineStart, timelineEnd),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay,
			expectedRawProbabilities,
			{
				userNowInMinute: function () { return 0; }
			}, options);

		// Only 1 margin at end of timeline since timeline is start from 00:00
		var expectedLengthPercentagePerMinute = 1 / ((timelineEnd - timelineStart) * 60 + constants.timelineMarginInMinutes);

		equal(probabilities.length, 3);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.width != undefined, true);
			equal(probabilities[i].styleJson.height == undefined, true);
		}

		var probability = probabilities[0];
		//equal(probability.styleJson.width, (((4 - 0) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
		equal(probability.tooltips().indexOf("00:00 - 04:00") > -1, true);
		equal(probability.cssClass(), "probability-low");

		probability = probabilities[1];
		//equal(probability.styleJson.width, (((7 - 6) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
		equal(probability.tooltips().indexOf("06:00 - 07:00") > -1, true);
		equal(probability.cssClass(), "probability-low");

		probability = probabilities[2];
		//equal(probability.styleJson.width, (((9 - 7) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
		equal(probability.tooltips().indexOf("07:00 - 09:00") > -1, true);
		equal(probability.cssClass(), "probability-high");
	});

	test("Should trim probability cell data periods according continousPeriods", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var timelineStart = 0, timelineEnd = 9;
		var scheduleDay = {
			FixedDate: baseDate,
			IsFullDayAbsence: false,
			IsDayOff: false,
			Periods: [
	 			{
	 				"StartTime": baseDate + "T00:00:00",
	 				"EndTime": baseDate + "T04:10:00"
	 			},
	 			{
	 				"StartTime": baseDate + "T08:50:00",
	 				"EndTime": baseDate + "T09:00:00"
	 			}
			]
		};
		//probability period is 24 hours
		var rawProbabilities = createRawProbabilities();
		var expectedRawProbabilities = rawProbabilities.filter(function (p) {
			return moment(p.EndTime) <= moment(baseDate).add(9, "hours");
		});

		expectedRawProbabilities.forEach(function (p) {
			p.Possibility = 1;
		});

		var options = {
			probabilityType: constants.absenceProbabilityType,
			layoutDirection: constants.horizontalDirectionLayout,
			mergeSameIntervals: true,
			hideProbabilityEarlierThanNow: false,
			timelines: createTimeline(timelineStart, timelineEnd),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay,
			expectedRawProbabilities,
			{
				userNowInMinute: function () { return 0; }
			}, options);

		equal(probabilities[probabilities.length - 2].tooltips().indexOf("00:00 - 04:10") > -1, true);
		equal(probabilities[probabilities.length - 1].tooltips().indexOf("08:50 - 09:00") > -1, true);
	});
});