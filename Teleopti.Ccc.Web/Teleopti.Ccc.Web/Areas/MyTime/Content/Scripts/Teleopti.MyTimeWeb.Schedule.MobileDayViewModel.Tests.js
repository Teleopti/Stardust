/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileDayViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileDayViewModel");

	Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var intervalLengthInMinute = 15;
	var fakeUserText = {
		xRequests: '@Resources.XRequests',
		probabilityForAbsence: '@Resources.ProbabilityToGetAbsenceColon',
		probabilityForOvertime: '@Resources.ProbabilityToGetOvertimeColon',
		hideStaffingInfo: '@Resources.HideStaffingInfo',
		showAbsenceProbability: '@Resources.ShowAbsenceProbability',
		showOvertimeProbability: '@Resources.ShowOvertimeProbability',
		staffingInfo: '@Resources.StaffingInfo'
	};
	var basedDate = moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD');

	function getFakeData(){
		return {
			PeriodSelection:{
				Display: basedDate
			},
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: basedDate,
					IsDayOff: false,
					IsFullDayAbsence: false,
					Header: {
						Title: 'Today',
						Date: basedDate,
						DayDescription: '',
						DayNumber: '18'
					},
					Note: {
						Message: ''
					},
					Summary: {
						Title: 'Early',
						TimeSpan: '09:30 - 16:45',
						StartTime: moment(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
						Summary: '7:15',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.0,
						EndPositionPercentage: 0.0,
						Color: 'rgb(128,255,128)',
						IsOvertime: false
					},
					OvertimeAvailabililty: {
						HasOvertimeAvailability: false,
						StartTime: null,
						EndTime: null,
						EndTimeNextDay: false,
						DefaultStartTime: '17:00',
						DefaultEndTime: '18:00',
						DefaultEndTimeNextDay: false
					},
					SeatBookings: [],
					Periods: [{
						'Title': 'Phone',
						'TimeSpan': '09:30 - 16:45',
						'StartTime': moment(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						'EndTime': moment(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
						'Summary': '7:15',
						'StyleClassName': 'color_80FF80',
						'Meeting': null,
						'StartPositionPercentage': 0.1896551724137931034482758621,
						'EndPositionPercentage': 1,
						'Color': '128,255,128',
						'IsOvertime': false
					}]
				}
			],
			Possibilities: [
				{
					Date: basedDate,
					StartTime: moment(basedDate).startOf('day').add('hour', 19).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
					EndTime: moment(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
					Possibility: 0
				}
			],
			CheckStaffingByIntraday: true,
			ViewPossibilityPermission: true,
			TimeLine: [{
					Time: "09:15:00",
					TimeLineDisplay: "09:15",
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: "17:00:00",
					TimeLineDisplay: "17:00",
					PositionPercentage: 1,
					TimeFixedFormat: null
				}],
			SiteOpenHourIntradayPeriod: null
			};
	}

	function fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter(){
		var result = [];
		for (var i = 0; i < 24 * 60 / intervalLengthInMinute; i++) {
			result.push({
				"StartTime": moment(basedDate).startOf('day').add(intervalLengthInMinute * i, "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				"EndTime": moment(basedDate).startOf('day').add(intervalLengthInMinute * (i + 1), "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				"Possibility": intervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	test("should read date", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], [], false, true, weekViewModel);

		equal(vm.fixedDate(), basedDate);
		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should read permission", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel({}, [], false, true, weekViewModel);

		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should load shift category data", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], [], true, true, weekViewModel);
		equal(vm.summaryName(), getFakeData().Days[0].Summary.Title);
		equal(vm.summaryTimeSpan(), getFakeData().Days[0].Summary.TimeSpan);
		equal(vm.summaryColor(), getFakeData().Days[0].Summary.Color);
		equal(vm.summaryStyleClassName(), getFakeData().Days[0].Summary.StyleClassName);
		equal(vm.backgroundColor, getFakeData().Days[0].Summary.Color);
	});

	test("should read dayoff data", function () {
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], [], true, true, weekViewModel);

		equal(vm.isDayoff(), true);
	});

	test("should indicate has shift", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], [], true, true, weekViewModel);
		equal(vm.hasShift, true);
	});

	test("should read week day header titles", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], [], true, true, weekViewModel);

		equal(vm.weekDayHeaderTitle(), "Today");
	});

	test("should read summary timespan when there is overtime and overtime availability", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], [], true, true, weekViewModel);
		equal(vm.summaryTimeSpan(), getFakeData().Days[0].Summary.TimeSpan);
		equal(vm.layers.length, 1);
	});

	test("should show no absence possibility if set to hide probability", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(getFakeData());
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], getFakeData().Possibilities, true, true, weekViewModel);
		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return false;
		};

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeData().Days[0], getFakeData().Possibilities, true, true, weekViewModel);

		equal(vm.probabilities.length, 0);
	});

	test("should show no absence possibility if agent has no permission", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.ViewPossibilityPermission = false;
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);

		equal(vm.probabilities.length, 0);
	});

	test("should show absence possibility within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show overtime possibility within timeline range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.TimeLine[1].Time = "15:00:00";		//narrow timeline to 9:30 - 15:00

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 14:45") > -1, true);
	});

	test("should show no absence possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		fakeData.Days[0].Periods = [];
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		equal(vm.probabilities().length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsFullDayAbsence = true;
		fakeData.Days[0].Periods[0].Title = "Full Day Absence";
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		equal(vm.probabilities().length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		fakeData.Days[0].Periods = [];
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);

		vm.userNowInMinute(0);
		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show overtime possibility based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 15:00") > -1, true);
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		fakeData.Days[0].IsDayOff = true;
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of initraday open hour
		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 15:00") > -1, true);
	});

	test("should show overtime possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		fakeData.Days[0].IsFullDayAbsence = true;
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("10:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 15:00") > -1, true);
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.TimeLine = [{
			Time: "00:00:00",
			TimeLineDisplay: "00:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "17:00:00",
			TimeLineDisplay: "17:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];
		fakeData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("00:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.TimeLine = [{
			Time: "00:00:00",
			TimeLineDisplay: "00:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "17:00:00",
			TimeLineDisplay: "17:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];
		fakeData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("00:00 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		weekViewModel.readData(fakeData);

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], fakeData.Possibilities, true, true, weekViewModel);
		// Will generate probabilities from schedule start (10:00) to schedule end (00:00+)
		equal(vm.probabilities().length, 2);
		equal(vm.probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(vm.probabilities()[1].tooltips().indexOf("12:00 - 00:00 +1") > -1, true);
	});
});