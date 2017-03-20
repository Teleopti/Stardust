/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.DayViewModel");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var expiredProbabilityCssClass = "probability-expired";

	var createTimeline = function (timelineStartHour, timelineEndHour) {
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

	var createRawProbabilities = function () {
		var result = [];
		var dateStart = "2017-02-16";
		for (var i = 0; i < 24 * 60 / 15; i++) {
			result.push({
				"StartTime": moment(dateStart).add(15 * i, "minutes").toDate(),
				"EndTime": moment(dateStart).add(15 * (i + 1), "minutes").toDate(),
				"Possibility": Math.round(Math.random())
			});
		}

		return result;
	}

	var createWeekViewmodel = function (probabilityType, timelineStartHour, timelineEndHour, intradayOpenPeriod) {
		return {
			"userTexts":
			{
				"xRequests": "{0} Request(s)",
				"fair": "Fair",
				"good": "Good",
				"probabilityForAbsence": "Probability to get absence:",
				"probabilityForOvertime": "Probability to get overtime:"
			},
			"staffingProbabilityEnabled": function () { return true; },
			"intradayOpenPeriod": intradayOpenPeriod,
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
				"StartTime": "2017-02-16T09:30:00",
				"EndTime": "2017-02-16T18:30:00",
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
				"StartTime": "2017-02-15T22:45:00",
				"EndTime": "2017-02-16T00:00:00",
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
				"StartTime": "2017-02-16T00:00:00",
				"EndTime": "2017-02-16T00:15:00",
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
				"StartTime": "2017-02-16T00:15:00",
				"EndTime": "2017-02-16T01:30:00",
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
				"StartTime": "2017-02-16T09:30:00",
				"EndTime": "2017-02-16T18:30:00",
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

	var createNightShiftPeriods = function () {
		return [{
			"Title": "Phone",
			"TimeSpan": "10:00 - 12:00",
			"StartTime": "2017-02-16T10:00:00",
			"EndTime": "2017-02-16T12:00:00",
			"Summary": "2:00",
			"StyleClassName": "color_80FF80",
			"Meeting": null,
			"StartPositionPercentage": 0.4166714892533478396740703017,
			"EndPositionPercentage": 0.5000057871040174076088843621,
			"Color": "128,255,128",
			"IsOvertime": false
		}, {
			"Title": "Short break",
			"TimeSpan": "12:00 - 12:15",
			"StartTime": "2017-02-16T12:00:00",
			"EndTime": "2017-02-16T12:15:00",
			"Summary": "0:15",
			"StyleClassName": "color_FF0000",
			"Meeting": null,
			"StartPositionPercentage": 0.5000057871040174076088843621,
			"EndPositionPercentage": 0.5104225743353511036007361196,
			"Color": "255,0,0",
			"IsOvertime": false
		}, {
			"Title": "Phone",
			"TimeSpan": "12:15 - 14:15",
			"StartTime": "2017-02-16T12:15:00",
			"EndTime": "2017-02-16T14:15:00",
			"Summary": "2:00",
			"StyleClassName": "color_80FF80",
			"Meeting": null,
			"StartPositionPercentage": 0.5104225743353511036007361196,
			"EndPositionPercentage": 0.59375687218602067153555018,
			"Color": "128,255,128",
			"IsOvertime": false
		}, {
			"Title": "Lunch",
			"TimeSpan": "14:15 - 15:15",
			"StartTime": "2017-02-16T14:15:00",
			"EndTime": "2017-02-16T15:15:00",
			"Summary": "1:00",
			"StyleClassName": "color_FFFF00",
			"Meeting": null,
			"StartPositionPercentage": 0.59375687218602067153555018,
			"EndPositionPercentage": 0.6354240211113554555029572102,
			"Color": "255,255,0",
			"IsOvertime": false
		}, {
			"Title": "Social Media",
			"TimeSpan": "15:15 - 17:00",
			"StartTime": "2017-02-16T15:15:00",
			"EndTime": "2017-02-16T17:00:00",
			"Summary": "1:45",
			"StyleClassName": "color_1E90FF",
			"Meeting": null,
			"StartPositionPercentage": 0.6354240211113554555029572102,
			"EndPositionPercentage": 0.708341531730691327445919513,
			"Color": "30,144,255",
			"IsOvertime": false
		}, {
			"Title": "Short break",
			"TimeSpan": "17:00 - 17:15",
			"StartTime": "2017-02-16T17:00:00",
			"EndTime": "2017-02-16T17:15:00",
			"Summary": "0:15",
			"StyleClassName": "color_FF0000",
			"Meeting": null,
			"StartPositionPercentage": 0.708341531730691327445919513,
			"EndPositionPercentage": 0.7187583189620250234377712705,
			"Color": "255,0,0",
			"IsOvertime": false
		}, {
			"Title": "Phone",
			"TimeSpan": "17:15 - 01:30 +1",
			"StartTime": "2017-02-16T17:15:00",
			"EndTime": "2017-02-17T01:30:00",
			"Summary": "8:15",
			"StyleClassName": "color_80FF80",
			"Meeting": null,
			"StartPositionPercentage": 0.7187583189620250234377712705,
			"EndPositionPercentage": 1.0,
			"Color": "128,255,128",
			"IsOvertime": false
		}];
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
		var week = createWeekViewmodel(constants.noneProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility if the feature is disabled", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.absenceProbabilityType, 2, 20);
		week.staffingProbabilityEnabled = function () { return false; }
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		equal(vm.probabilities.length, 0);
	});

	test("should show no overtime possibility if the feature is disabled", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20);
		week.staffingProbabilityEnabled = function () { return false; }
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		equal(vm.probabilities.length, 0);
	});

	test("should show absence possibility within schedule time range", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.absenceProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// Total 9 hours * 4 = 36 periods
		equal(vm.probabilities.length, 36);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i > 0) {
				equal(probability.tooltips().length > 0, true);
			}
		}
	});

	test("should show overtime possibility within timeline range", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 8, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// Will generate all overtime possibility within timeline range (from 08:00 to 19:00)
		equal(vm.probabilities.length, 46);
		for (var i = 0; i < vm.probabilities.length; i++) {
			equal(vm.probabilities[i].tooltips().length > 0, true);
		}
	});

	test("should hide absence possibility earlier than now", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());

		var week = createWeekViewmodel(constants.absenceProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(750); // 12:30

		// Total 9 hours * 4 = 36 periods
		equal(vm.probabilities.length, 36);

		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			// Schedule started from 09:30, current time is 12:30
			// Then the first (12:30 - 09:30) * 4 = 12 probabilities should be masked
			if (i < 12)
				equal(probability.tooltips().length > 0, false);
			else
				equal(probability.tooltips().length > 0, true);
		}
	});

	test("should hide overtime possibility earlier than now", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(750); // 12:30

		// Will generate all overtime possibility within timeline range (from 02:00 to 20:00)
		equal(vm.probabilities.length, 74);

		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i <= 42){
				equal(probability.tooltips().length, 0);
				equal(probability.cssClass().indexOf(expiredProbabilityCssClass) > -1, true);
			} else {
				// Open hour period started from 02:00, current time is 12:30
				// Then the first (01:45 12:30) * 4 =  probabilities should be invisible
				equal(probability.cssClass().indexOf(expiredProbabilityCssClass), -1);
				equal(probability.tooltips().length > 0, true);
			}
		}
	});

	test("should show no absence possibility for dayoff", function () {
		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(constants.absenceProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(constants.absenceProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of timeline
		// So should be (20 - 2) * 4
		equal(vm.probabilities.length, 74);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			equal(probability.tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility based on intraday open hour", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var intradayOpenHour = {
			"startTime": "10:00:00",
			"endTime": "15:00:00"
		};
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20, intradayOpenHour);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// Only probability within open hour period will be generated, so there will be (15 - 10) * 4 probabilities
		equal(vm.probabilities.length, 20);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			equal(probability.tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		var day = createRawDaySchedule(true, false, creatPeriods());
		var intradayOpenHour = {
			"startTime": "10:00:00",
			"endTime": "15:00:00"
		};
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20, intradayOpenHour);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of initraday open houru
		// So should be (15 - 10) * 4 
		equal(vm.probabilities.length, 20);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			equal(probability.tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for fullday absence", function () {
		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of timeline
		// So should be (20 - 2) * 4
		equal(vm.probabilities.length, 74);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			equal(probability.tooltips().length > 0, true);
		}
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		var day = createRawDaySchedule(false, false, createCrossDayPeriods());
		var week = createWeekViewmodel(constants.overtimeProbabilityType, 0, 19);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
		vm.userNowInMinute(0);

		// In this scenario timeline will start from 00:00, then all probabilities will be generated for whole timeline
		// So should be (19 - 0) * 4
		equal(vm.probabilities.length, 77);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			equal(probability.tooltips().length > 0, true);
		}
	});

	// test("should show correct absence possibility for cross day schedule", function () {
	// 	var day = createRawDaySchedule(false, false, createCrossDayPeriods());
	// 	var week = createWeekViewmodel(constants.absenceProbabilityType, 0, 19);
	// 	var probabilities = createRawProbabilities();
	// 	var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);
	// 	vm.userNowInMinute(0);

	// 	// In this scenario timeline will start from 00:00
	// 	// So should be (18:30 - 0) * 4
	// 	equal(vm.probabilities.length, 74);

	// 	// Probability from 01:30 to 09:30 should be invisible since there is no schedule for this time range
	// 	for (var i = 0; i < vm.probabilities.length; i++) {
	// 		var probability = vm.probabilities[i];
	// 		if ((7 <= i && i <= 38)) {

	// 			equal(probability.tooltips().length, 0);
	// 		} else {

	// 			equal(probability.tooltips().length > 0, true);
	// 		}
	// 	}
	// });

	test("should show absence possibility for night shift schedule", function () {
		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(constants.absenceProbabilityType, 0, 24);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(day, probabilities, week);

		// Will generate probabilities from schedule start (10:00) to schedule end (00:00+)
		equal(vm.probabilities.length, 56);
	});
});