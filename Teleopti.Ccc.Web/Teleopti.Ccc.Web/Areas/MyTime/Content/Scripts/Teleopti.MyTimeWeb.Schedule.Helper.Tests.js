/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.Helper");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var yesterday = "2017-03-09";
	var baseDate = "2017-03-10";
	var tomorrow = "2017-03-11";

	function createTimeline (timelineStartHour, timelineEndHour) {
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

	function createRawProbabilities () {
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
		equal(vm[0].startTime, 165);
		equal(vm[0].endTime, 360);
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
		equal(firstContinousPeriod.startTime, 165);
		equal(firstContinousPeriod.endTime, 255);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTime, 360);
		equal(secondContinousPeriod.endTime, 540);
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
		equal(firstContinousPeriod.startTime, 165);
		equal(firstContinousPeriod.endTime, 255);

		var secondContinousPeriod = vm[1];
		equal(secondContinousPeriod.startTime, 360);
		equal(secondContinousPeriod.endTime, 540);

		var thirdContinousPeriod = vm[2];
		equal(thirdContinousPeriod.startTime, 840);
		equal(thirdContinousPeriod.endTime, 1200);
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

	test("Should not create probability if set to show no probability", function () {
		var scheduleDay = {};
		var rawProbability = createRawProbabilities();
		var options = {
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
			probabilityType: constants.absenceProbabilityType
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
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
			mergeIntervals: false,
			timelines: createTimeline(1, 8),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);

		equal(probabilities.length, 14);
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
			mergeIntervals: false,
			timelines: createTimeline(1, 8),
			userTexts: {
				"high": "High",
				"low": "Low",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			}
		};

		var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, rawProbability, {}, options);
		equal(probabilities.length, 14);
		for (var i = 0; i < probabilities.length; i++) {
			equal(probabilities[i].styleJson.width != undefined, true);
			equal(probabilities[i].styleJson.height == undefined, true);
		}
	});

//test("")


	// test("Should merge probability cells with identical intervals when options.mergeIntervals is set to true", function () {
	// 	var scheduleDay = {
	// 		FixedDate: baseDate,
	// 		IsFullDayAbsence: false,
	// 		IsDayOff: false,
	// 		Periods: [
	// 			{
	// 				"StartTime": baseDate + "T06:00:00",
	// 				"EndTime": baseDate + "T09:00:00"
	// 			},
	// 		]
	// 	};
	// 	var rawProbabilities = createRawProbabilities();

	// 	rawProbabilities.forEach(function(p, index){
	// 		//Test case: probability level is low from 6:00 to 7:00, and high between 7:00 to 9:00
	// 		//Schedule Period: 6:00 ~ 9:00, Timeline: 0:45 ~ 9:15
	// 		if(index < (7 * 4)){
	// 			p.Possibility = 0;
	// 		}else{
	// 			p.Possibility = 1;
	// 		}
	// 	});
	// 	var expectedRawProbabilities = rawProbabilities.filter(function(p){
	// 		return moment(p.EndTime) <= moment(baseDate).add(9,'hours');
	// 	});

	// 	var options = {
	// 		probabilityType: constants.absenceProbabilityType,
	// 		layoutDirection: constants.horizontalDirectionLayout,
	// 		mergeIntervals: true,
	// 		timelines: createTimeline(1, 9),
	// 		userTexts: {
	// 			"high": "High",
	// 			"low": "Low",
	// 			"probabilityForAbsence": "Probability to get absence:",
	// 			"probabilityForOvertime": "Probability to get overtime:"
	// 		}
	// 	};

	// 	var probabilities = Teleopti.MyTimeWeb.Schedule.Helper.CreateProbabilityModels(scheduleDay, expectedRawProbabilities, {userNowInMinute: function(){return 0;}}, options);

	// 	var expectedLengthPercentagePerMinute = 1/((9 * 60 + 15) - (1 * 60 - 15));

	// 	equal(probabilities.length, 3);
	// 	for (var i = 0; i < probabilities.length; i++) {
	// 		equal(probabilities[i].styleJson.width != undefined, true);
	// 		equal(probabilities[i].styleJson.height == undefined, true);
	// 	}

	// 	//equal(probabilities[0].styleJson.width, (((6 - 1) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
	// 	equal(probabilities[0].tooltips().length == 0, true);

	// 	//equal(probabilities[1].styleJson.width, (((7 - 6) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
	// 	equal(probabilities[1].tooltips().indexOf('6:00') > -1, true);
	// 	equal(probabilities[1].tooltips().indexOf('7:00') > -1, true);

	// 	//equal(probabilities[2].styleJson.width, (((9 - 7) * 60 * expectedLengthPercentagePerMinute * 100) + "%"));
	// 	equal(probabilities[2].tooltips().indexOf('7:00') > -1, true);
	// 	equal(probabilities[2].tooltips().indexOf('9:00') > -1, true);
	// });
});