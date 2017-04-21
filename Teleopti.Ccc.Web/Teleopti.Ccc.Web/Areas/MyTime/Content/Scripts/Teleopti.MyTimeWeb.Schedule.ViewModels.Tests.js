/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.DayViewModel");

	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var expiredProbabilityCssClass = "probability-expired";

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

	var fakeAddRequestViewModel = function() {
		return {
			DateFormat: function() {
				return 'YYYY-MM-DD';
			}
		};
	};
	var basedDate = moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD');

	function getFakeData(){
		return {
			PeriodSelection: null,
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

	test("should show no absence possibility if set to hide probability", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);

		week.probabilityType(constants.probabilityType.none);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		equal(vm.probabilities().length, 0);
	});

	test("should show no absence possibility if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return false;
		};
		var fakeData = getFakeData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		equal(vm.probabilities.length, 0);
	});

	test("should show no overtime possibility if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return false;
		};
		var fakeData = getFakeData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);

		equal(vm.probabilities.length, 0);
	});

	test("should show absence possibility within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		// Total 9:30 ~ 16:45 = 29 intervals
		equal(vm.probabilities().length, 29);
		for (var i = 0; i < vm.probabilities.length; i++) {
			var probability = vm.probabilities[i];
			if (i > 0) {
				equal(probability.tooltips().length > 0, true);
			}
		}
	});

	test("should show overtime possibility within timeline range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.TimeLine = [{
			Time: "00:00:00",
			TimeLineDisplay: "00:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "24:00:00",
			TimeLineDisplay: "00:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 96);
		for (var i = 0; i < vm.probabilities.length; i++) {
			equal(vm.probabilities[i].tooltips().length > 0, true);
		}
	});

	test("should hide absence possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(750); // 12:30

		equal(vm.probabilities().length, 29);

		for (var i = 0; i < vm.probabilities().length; i++) {
			// Schedule started from 09:30, current time is 12:30
			// Then the first (09:30 - 12:30) * 4 = 12 probabilities should be masked
			if (i < 12)
				equal(vm.probabilities()[i].tooltips().length > 0, false);
			else
				equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should hide overtime possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(750); // 12:30

		equal(vm.probabilities().length, 29);
		for (var i = 0; i < vm.probabilities().length; i++) {
			var probability = vm.probabilities()[i];
			//9:30 ~ 12:30 = 3 * 4 intervals
			if (i < 12) {
				equal(probability.tooltips().length, 0);
				equal(probability.cssClass().indexOf(expiredProbabilityCssClass) > -1, true);
			} else {
				equal(probability.cssClass().indexOf(expiredProbabilityCssClass), -1);
				equal(probability.tooltips().length > 0, true);
			}
		}
	});

	test("should show no absence possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);
		equal(vm.probabilities().length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		equal(vm.probabilities().length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.TimeLine = [{
			Time: "08:00:00",
			TimeLineDisplay: "08:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "18:00:00",
			TimeLineDisplay: "18:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);
		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(vm.probabilities().length, 38);
		for (var i = 0; i < vm.probabilities().length; i++) {
			equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		equal(vm.probabilities().length, 20);
		for (var i = 0; i < vm.probabilities().length; i++) {
			equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsDayOff = true;
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		// In this scenario will show prabability based on length of intraday open hour
		// So should be (15 - 10) * 4
		equal(vm.probabilities().length, 20);
		for (var i = 0; i < vm.probabilities().length; i++) {
			equal( vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].IsFullDayAbsence = true;
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.TimeLine = [{
			Time: "08:00:00",
			TimeLineDisplay: "08:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "18:00:00",
			TimeLineDisplay: "18:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(vm.probabilities().length, 38);
		for (var i = 0; i < vm.probabilities().length; i++) {
			equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		fakeData.TimeLine = [{
			Time: "00:00:00",
			TimeLineDisplay: "00:00",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "18:00:00",
			TimeLineDisplay: "18:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.overtime);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		//18 * 4 - 1 margin = 71
		equal(vm.probabilities().length, 71);
		for (var i = 0; i < vm.probabilities().length; i++) {
			equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		//according to scheduel period 00:00 ~ 16:45 = 16.75 * 4 = 
		equal(vm.probabilities().length, 67);

		for (var i = 0; i < vm.probabilities().length; i++) {
			equal(vm.probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.TimeLine = [{
			Time: "09:15:00",
			TimeLineDisplay: "09:15",
			PositionPercentage: 0,
			TimeFixedFormat: null
		},
		{
			Time: "24:00:00",
			TimeLineDisplay: "24:00",
			PositionPercentage: 1,
			TimeFixedFormat: null
		}];
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeData);
		week.probabilityType(constants.probabilityType.absence);

		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], fakeData.Possibilities, week);
		vm.userNowInMinute(0);

		//9:30 ~ 24:00 = 14.5 * 4 = 58
		equal(vm.probabilities().length, 58);

	});
});