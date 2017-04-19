/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileDayViewModel");

	Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

	var constants = Teleopti.MyTimeWeb.Common.Constants;

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
	};

	var createRawProbabilities = function (generateProbability) {
		var result = [];
		var intervalLengthInMinute = 15;
		var dateStart = "2017-02-16";

		for (var i = 0; i < 24 * 60 / intervalLengthInMinute; i++) {
			result.push({
				"StartTime": moment(dateStart).add(intervalLengthInMinute * i, "minutes").toDate(),
				"EndTime": moment(dateStart).add(intervalLengthInMinute * (i + 1), "minutes").toDate(),
				"Possibility": generateProbability == undefined
					? Math.round(Math.random())
					: generateProbability(intervalLengthInMinute * i)
			});
		}

		return result;
	};

	var createWeekViewmodel = function (probabilityType, timelineStartHour, timelineEndHour, intradayOpenPeriod) {
		var self = this;
		self.userTexts = {
			"xRequests": "{0} Request(s)",
			"low": "Low",
			"high": "High",
			"probabilityForAbsence": "Probability to get absence:",
			"probabilityForOvertime": "Probability to get overtime:"
		};
		self.staffingProbabilityOnMobileEnabled = function () { return true; };
		self.staffingProbabilityForMultipleDaysEnabled = function () { return false; };
		self.absenceProbabilityEnabled = ko.observable(true);
		self.intradayOpenPeriod = intradayOpenPeriod;
		self.textPermission = function () { return true; };
		self.requestPermission = function () { return true; };
		self.absenceRequestPermission = function () { return true; };
		self.currentUserDate = ko.observable(moment("2017-02-16").startOf("day"));
		self.formatedCurrentUserDate = ko.observable(moment("2017-02-16").startOf("day").format('l'));
		self.dayViewModels = ko.observableArray();
		self.absenceReportPermission = ko.observable();
		self.requestViewModel = ko.observable();
		self.initialRequestDay = ko.observable();
		self.formattedRequestDate = ko.computed(function () {
			return moment(self.initialRequestDay()).format("YYYY-MM-DD");
		});

		self.showingAbsenceProbability = ko.observable(false);
		self.showingOvertimeProbability = ko.observable(false);
		self.selectedProbabilityOptionValue = ko.observable(probabilityType ? probabilityType : constants.probabilityType.none);
		self.OnProbabilityOptionSelectCallback = function (selectedOptionValue) {
			self.selectedProbabilityOptionValue(selectedOptionValue);
			if (selectedOptionValue === 0) {
				self.requestViewModel(undefined);
			}
		};

		self.createProbabilityOptionModel = function () {
			return {
				model: new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self),
				type: function () { return "probabilityOptions" },
				OnProbabilityOptionSelectCallback: function (selectedOptionValue) {
					self.OnProbabilityOptionSelectCallback(selectedOptionValue);
				}
			};
		};

		self.probabilityOptionModel = ko.observable(self.createProbabilityOptionModel());

		self.toggleProbabilityOptionsPanel = function (data) {
			self.initialRequestDay(data.fixedDate());

			if (self.requestViewModel() && self.requestViewModel().type() === self.probabilityOptionModel.type()) {
				if (self.requestViewModel().model.checkedProbability() === 0) {
					self.requestViewModel(undefined);
					self.showingAbsenceProbability(false);
					self.showingOvertimeProbability(false);
				}
			} else {
				self.requestViewModel(self.probabilityOptionModel());
			}
		};
		self.styles = function () {
			return {
				"color_80FF80": "rgb(128,255,128)",
				"color_FF0000": "rgb(255,0,0)",
				"color_FFFF00": "rgb(255,255,0)",
				"color_C0C0FF": "rgb(192,192,255)",
				"color_FF8080": "rgb(255,128,128)",
				"color_1E90FF": "rgb(30,144,255)",
				"color_FFC080": "rgb(255,192,128)"
			};
		};
		self.days = function () { return []; };
		self.probabilityType = function () { return probabilityType; };
		self.timeLines = function () { return createTimeline(timelineStartHour, timelineEndHour); };
		return self;
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
	};

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
	};

	var createNightShiftPeriods = function () {
		return [
			{
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
			}
		];
	};

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

	test("should read date", function () {
		var scheduleDay = {
			FixedDate: "2014-04-14"
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], false, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));

		equal(vm.fixedDate(), "2014-04-14");
		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should read permission", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel({}, [], false, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));

		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should load shift category data", function () {
		var scheduleDay = {
			Summary: {
				Title: "Early",
				TimeSpan: "09:00-18:00",
				StyleClassName: "dayoff striped",
				Color: "rgb(0, 0, 0)"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], true, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));
		equal(vm.summaryName(), scheduleDay.Summary.Title);
		equal(vm.summaryTimeSpan(), scheduleDay.Summary.TimeSpan);
		equal(vm.summaryColor(), scheduleDay.Summary.Color);
		equal(vm.summaryStyleClassName(), scheduleDay.Summary.StyleClassName);
		equal(vm.backgroundColor, scheduleDay.Summary.Color);
	});

	test("should read dayoff data", function () {
		var scheduleDay = {
			Summary: {
				StyleClassName: "dayoff striped"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], true, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));
		equal(vm.isDayoff(), true);
	});

	test("should indicate has shift", function () {
		var scheduleDay = {
			Summary: {
				Color: ""
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], true, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));
		equal(vm.hasShift, true);
	});

	test("should read week day header titles", function () {
		var scheduleDay = {
			Header: {
				Title: "Monday"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], true, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));
		equal(vm.weekDayHeaderTitle(), "Monday");
	});

	test("should read summary timespan when there is overtime and overtime availability", function () {
		var scheduleDay = {
			Summary: {
				Color: null,
				TimeSpan: null
			},
			Periods: [
				{
					IsOvertime: true,
					TimeSpan: "9:00 AM - 11:30 AM",
					Title: "Phone"
				},
				{
					IsOvertime: true,
					TimeSpan: "11:30 AM - 11:45 AM",
					Title: "Short Break"
				},
				{
					IsOvertime: true,
					TimeSpan: "11:45 AM - 2:00 PM",
					Title: "Phone"
				},
				{
					IsOvertimeAvailability: true,
					TimeSpan: "7:00 AM - 2:00 PM",
					Title: "Overtime Availability"
				}
			],
			HasOvertime: true
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, [], true, true, createWeekViewmodel(constants.probabilityType.none, 2, 20));
		equal(vm.summaryTimeSpan(), "9:00 AM -  2:00 PM");
		equal(vm.layers.length, 4);
	});

	test("should show no absence possibility if set to hide probability", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.none, 2, 20);
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility if the feature is disabled", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 2, 20);
		week.staffingProbabilityOnMobileEnabled = function () { return false; }
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		equal(vm.probabilities.length, 0);
	});

	test("should show no overtime possibility if the feature is disabled", function () {
		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.overtime, 2, 20);
		week.staffingProbabilityOnMobileEnabled = function () { return false; }
		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		equal(vm.probabilities.length, 0);
	});

	test("should show absence possibility within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 2, 20);
		var rawProbabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 ? 0 : 1;
		});

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, rawProbabilities, true, true, week);

		equal(vm.probabilities().length, 2);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}

		equal(vm.probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 18:30") > -1, true);
	});

	test("should show overtime possibility within timeline range", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.overtime, 8, 19);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 ? 0 : 1;
		});

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		// Will generate all overtime possibility within timeline range (from 08:00 to 19:00)
		equal(vm.probabilities().length, 2);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}

		equal(vm.probabilities()[0].tooltips().indexOf("08:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 19:00") > -1, true);
	});

	test("should show no absence possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 2, 20);
		var probabilities = createRawProbabilities();

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		equal(vm.probabilities().length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 2, 20);
		var probabilities = createRawProbabilities();

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		equal(vm.probabilities().length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(true, false, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.overtime, 2, 20);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 ? 0 : 1;
		});

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}
		equal(vm.probabilities()[0].tooltips().indexOf("02:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 20:00") > -1, true);
	});

	test("should show overtime possibility based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, creatPeriods());
		var intradayOpenHour = {
			"startTime": "10:00:00",
			"endTime": "15:00:00"
		};
		var week = createWeekViewmodel(constants.probabilityType.overtime, 2, 20, intradayOpenHour);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 ? 0 : 1;
		});

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var tooltips = vm.probabilities()[i].tooltips();
			equal(tooltips.length > 0, true);
		}
		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 15:00") > -1, true);
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(true, false, creatPeriods());
		var intradayOpenHour = {
			"startTime": "10:00:00",
			"endTime": "15:00:00"
		};
		var week = createWeekViewmodel(constants.probabilityType.overtime, 2, 20, intradayOpenHour);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 + 30 || intervalStartMinute >= 14 * 60 ? 0 : 1;
		});
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of initraday open houru
		// So should be (15 - 10) * 4
		equal(vm.probabilities().length, 3);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}
		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:30") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:30 - 14:00") > -1, true);
		equal(vm.probabilities()[2].tooltips().indexOf("14:00 - 15:00") > -1, true);
	});

	test("should show overtime possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, true, creatPeriods());
		var week = createWeekViewmodel(constants.probabilityType.overtime, 2, 20);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 + 30 || intervalStartMinute >= 14 * 60 ? 0 : 1;
		});
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 3);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];

			equal(probability.tooltips().length > 0, true);
		}
		equal(vm.probabilities()[0].tooltips().indexOf("02:00 - 12:30") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:30 - 14:00") > -1, true);
		equal(vm.probabilities()[2].tooltips().indexOf("14:00 - 20:00") > -1, true);
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, createCrossDayPeriods());
		var week = createWeekViewmodel(constants.probabilityType.overtime, 0, 19);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 + 30 || intervalStartMinute >= 14 * 60 ? 0 : 1;
		});
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 3);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}
		equal(vm.probabilities()[0].tooltips().indexOf("00:00 - 12:30") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:30 - 14:00") > -1, true);
		equal(vm.probabilities()[2].tooltips().indexOf("14:00 - 19:00") > -1, true);
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, createCrossDayPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 0, 19);
		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 + 30 || intervalStartMinute >= 14 * 60 ? 0 : 1;
		});

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 4);

		// Probability from 01:30 to 09:30 should be invisible since there is no schedule for this time range
		// will not create invisible probability view models
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}

		equal(vm.probabilities()[0].tooltips().indexOf("00:00 - 01:30") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("09:30 - 12:30") > -1, true);
		equal(vm.probabilities()[2].tooltips().indexOf("12:30 - 14:00") > -1, true);
		equal(vm.probabilities()[3].tooltips().indexOf("14:00 - 18:30") > -1, true);
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(constants.probabilityType.absence, 0, 24);

		var probabilities = createRawProbabilities(function generateProbability(intervalStartMinute) {
			return intervalStartMinute < 12 * 60 + 30 || intervalStartMinute >= 14 * 60 ? 0 : 1;
		});
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		// Will generate probabilities from schedule start (10:00) to schedule end (00:00+)
		equal(vm.probabilities().length, 3);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			equal(probability.tooltips().length > 0, true);
		}

		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:30") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:30 - 14:00") > -1, true);
		equal(vm.probabilities()[2].tooltips().indexOf("14:00 - 00:00 +1") > -1, true);
	});

	test("should set default probability option to hidden ", function () {
		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(undefined, 0, 24);

		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		week.toggleProbabilityOptionsPanel(vm);
		equal(week.requestViewModel().model.checkedProbability(), 0);
	});

	test("should change probability option value to 1 after selecting Show absence probability ", function () {
		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(constants.probabilityType.none, 0, 24);

		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		week.toggleProbabilityOptionsPanel(vm);
		equal(week.requestViewModel().model.checkedProbability(), 0);

		week.requestViewModel().model.onOptionSelected(1);
		week.probabilityOptionModel(week.createProbabilityOptionModel());
		equal(week.requestViewModel().model.checkedProbability(), 1);
	});

	test("should change staffing probability option value to 2 after selecting Show overtime  probability ", function () {
		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(constants.probabilityType.none, 0, 24);

		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);

		week.toggleProbabilityOptionsPanel(vm);
		equal(week.requestViewModel().model.checkedProbability(), 0);

		week.requestViewModel().model.onOptionSelected(2);
		week.probabilityOptionModel(week.createProbabilityOptionModel());
		equal(week.requestViewModel().model.checkedProbability(), 2);
	});

	test("should toggle off staffing probability after selecting Hide staffing probability ", function () {
		var day = createRawDaySchedule(false, false, createNightShiftPeriods());
		var week = createWeekViewmodel(constants.probabilityType.none, 0, 24);

		var probabilities = createRawProbabilities();
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(day, probabilities, true, true, week);
		week.selectedProbabilityOptionValue(1);

		week.probabilityOptionModel(week.createProbabilityOptionModel());
		week.toggleProbabilityOptionsPanel(vm);
		equal(week.requestViewModel().model.checkedProbability(), 1);

		week.requestViewModel().model.onOptionSelected(0);
		equal(week.requestViewModel(), undefined);
	});
});