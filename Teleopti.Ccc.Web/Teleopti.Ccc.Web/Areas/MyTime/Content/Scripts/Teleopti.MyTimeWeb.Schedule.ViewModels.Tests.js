/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.DayViewModel");

	var noneProbabilityType = 0;
	var absenceProbabilityType = 1;
	var overtimeProbabilityType = 2;

	var createTimeline = function (timelineStartHour, timelineEndHour) {
		var timelinePoints = [];
		var startHour = timelineStartHour;
		var endHour = timelineEndHour;
		for (var i = startHour; i <= endHour; i++) {
			timelinePoints.push({
				"minutes": i * 60,
				"timeText": i + ":00"
			});
		}

		return timelinePoints;
	}

	var createRawProbabilities = function () {
		var result = [];
		var dateStart = "2017-02-13";
		for (var i = 0; i < 24 * 60 / 15; i++) {
			result.push({
				"StartTime": moment(dateStart).add(15 * i, "minutes").toDate(),
				"EndTime": moment(dateStart).add(15 * (i + 1), "minutes").toDate(),
				"Possibility": Math.round(Math.random())
			});
		}

		return result;
	}

	var createWeekViewmodel = function (probabilityType, timelineStartHour, timelineEndHour) {
		return {
			"userTexts":
			{
				"xRequests": "{0} Request(s)",
				"fair": "Fair",
				"good": "Good",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			},
			"intradayOpenPeriod": null,
			"textPermission": function () { return true; },
			"requestPermission": function () { return true; },
			"absenceRequestPermission": function () { return true; },
			"styles": function () {
				return {
					"color_80FF80": "rgb(128,255,128)",
					"color_FF0000": "rgb(255,0,0)",
					"color_FFFF00": "rgb(255,255,0)",
					"color_C0C0FF": "rgb(192,192,255)",
					"color_FF8080": "rgb(255,128,128)",
					"color_1E90FF": "rgb(30,144,255)",
					"color_FFC080": "rgb(255,192,128)"
				};
			},
			"days": function () { return []; },
			"probabilityType": function () { return probabilityType; },
			"timeLines": function () { return createTimeline(timelineStartHour, timelineEndHour); }
		}
	};

	var creatPeriods = function () {
		return [
			{
				"Title": "Phone",
				"TimeSpan": "09:30 - 18:30",
				"Summary": "8:00",
				"StyleClassName": "color_80FF80",
				"Meeting": null,
				"StartPositionPercentage": 0.1590909090909091,
				"EndPositionPercentage": 0.9772727272727273,
				"Color": "128,255,128",
				"IsOvertime": false
			}
		];
	}

	var createCrossDayPeriods = function () {
		return [
			{
				"Title": "Phone",
				"TimeSpan": "22:45 - 00:00 +1",
				"Summary": "1:15",
				"StyleClassName": "color_80FF80",
				"Meeting": null,
				"StartPositionPercentage": 0.0,
				"EndPositionPercentage": 0.0,
				"Color": "128,255,128",
				"IsOvertime": false
			},
			{
				"Title": "Short break",
				"TimeSpan": "00:00 - 00:15",
				"Summary": "0:15",
				"StyleClassName": "color_FF0000",
				"Meeting": null,
				"StartPositionPercentage": 0.0,
				"EndPositionPercentage": 0.0104167872313336959918517575,
				"Color": "255,0,0",
				"IsOvertime": false
			},
			{
				"Title": "Email",
				"TimeSpan": "00:15 - 01:30",
				"Summary": "1:15",
				"StyleClassName": "color_80FF80",
				"Meeting": null,
				"StartPositionPercentage": 0.0104167872313336959918517575,
				"EndPositionPercentage": 0.0625007233880021759511105453,
				"Color": "128,255,128",
				"IsOvertime": false
			},
			{
				"Title": "Phone",
				"TimeSpan": "09:30 - 18:30",
				"Summary": "9:00",
				"StyleClassName": "color_80FF80",
				"Meeting": null,
				"StartPositionPercentage": 0.3958379147906804476903667867,
				"EndPositionPercentage": 0.7708422551186935033970300582,
				"Color": "128,255,128",
				"IsOvertime": false
			}
		];
	}

	var createRawDaySchedule = function (isDayoff, isFullDayAbsence, periods) {
		return {
			"TextRequestCount": 0,
			"Date": "16/02/2017",
			"FixedDate": "2017-02-16",
			"State": 0,
			"Header":
			{
				"Title": "Thursday",
				"Date": "16/02/2017",
				"DayDescription": "",
				"DayNumber": "16"
			},
			"Note":
			{
				"Message": ""
			},
			"OvertimeAvailabililty":
			{
				"HasOvertimeAvailability": false,
				"StartTime": null,
				"EndTime": null,
				"EndTimeNextDay": false,
				"DefaultStartTime": "18:30",
				"DefaultEndTime": "19:30",
				"DefaultEndTimeNextDay": false
			},
			"HasOvertime": false,
			"IsFullDayAbsence": isFullDayAbsence,
			"IsDayOff": isDayoff,
			"Summary":
			{
				"Title": "Day",
				"TimeSpan": "09:30 - 18:30",
				"Summary": "8:00",
				"StyleClassName": "color_FFC080",
				"Meeting": null,
				"StartPositionPercentage": 0,
				"EndPositionPercentage": 0,
				"Color": "rgb(255,192,128)",
				"IsOvertime": false
			},
			"Periods": periods,
			"DayOfWeekNumber": 4,
			"Availability": true,
			"HasNote": false,
			"ProbabilityClass": "red",
			"ProbabilityText": "Poor",
			"SeatBookings": []
		}
	};

	test("should show no absence possibility if set to hide probability", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(noneProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show absence possibility within schedule time range", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(absenceProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// Total 9 hours * 4 = 36 periods, and there is one extra period at the end
		equal(vm.probabilities.length, 37);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i === 0) {
				equal(probability.cssClass, "probability-none");
				equal(probability.tooltips.length, 0);
			} else {
				notEqual(probability.cssClass, "probability-none");
				equal(probability.tooltips.length > 0, true);
			}
		}
	});

	test("should show no absence possibility for dayoff", function () {
		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(absenceProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(absenceProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(overtimeProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// In this scenario will show prabability based on length of timeline
		// So should be (19 - 8) * 4 + 1
		equal(vm.probabilities.length, 45);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i === 0) {
				equal(probability.cssClass, "probability-none");
				equal(probability.tooltips.length, 0);
			} else {
				notEqual(probability.cssClass, "probability-none");
				equal(probability.tooltips.length > 0, true);
			}
		}
	});

	test("should show no absence possibility for fullday absence", function () {
		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(overtimeProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// In this scenario will show prabability based on length of timeline
		// So should be (19 - 8) * 4 + 1
		equal(vm.probabilities.length, 45);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i === 0) {
				equal(probability.cssClass, "probability-none");
				equal(probability.tooltips.length, 0);
			} else {
				notEqual(probability.cssClass, "probability-none");
				equal(probability.tooltips.length > 0, true);
			}
		}
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		var day = createRawDaySchedule(false, false, createCrossDayPeriods());
		var week = createWeekViewmodel(overtimeProbabilityType, 0, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// In this scenario timeline will start from 00:00
		// So should be (18.5 - 0) * 4 + 1
		equal(vm.probabilities.length, 75);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i === 0) {
				equal(probability.cssClass, "probability-none");
				equal(probability.tooltips.length, 0);
			} else {
				notEqual(probability.cssClass, "probability-none");
				equal(probability.tooltips.length > 0, true);
			}
		}
	});

	test("should show correct absence possibility for cross day schedule", function () {
		var day = createRawDaySchedule(false, false, createCrossDayPeriods());
		var week = createWeekViewmodel(absenceProbabilityType, 0, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// In this scenario timeline will start from 00:00
		// So should be (18.5 - 0) * 4 + 1
		equal(vm.probabilities.length, 75);

		// Probability from 01:30 to 09:30 should be invisible since there is no schedule for this time range
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i === 0 || (7 <= i && i <= 38)) {
				equal(probability.cssClass, "probability-none");
				equal(probability.tooltips.length, 0);
			} else {
				notEqual(probability.cssClass, "probability-none");
				equal(probability.tooltips.length > 0, true);
			}
		}
	});
});