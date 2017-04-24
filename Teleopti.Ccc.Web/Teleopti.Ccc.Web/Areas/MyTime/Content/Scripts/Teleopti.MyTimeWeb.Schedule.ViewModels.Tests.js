﻿/// <reference path="~/Content/Scripts/qunit.js" />
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

	function getFakeScheduleData(){
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
			CheckStaffingByIntraday: true,
			ViewPossibilityPermission: true,
			RequestPermission: {
				TextRequestPermission: true,
				AbsenceRequestPermission: true,
				ShiftTradeRequestPermission: false,
				OvertimeAvailabilityPermission: true,
				AbsenceReportPermission: true,
				ShiftExchangePermission: true,
				ShiftTradeBulletinBoardPermission: true,
				PersonAccountPermission: true
			},
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

	function getFakeProbabilityData(){
		return [
			{
				Date: basedDate,
				StartTime: moment(basedDate).startOf('day').add('hour', 19).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 0
			}
		];
	}

	test("should read date", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		equal(vm.fixedDate(), basedDate);
	});

	test("should read permission", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		equal(vm.textRequestPermission(), true);
		equal(vm.requestPermission(), true);
	});

	test("should load shift category data", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);
		equal(vm.summaryTitle(), fakeScheduleData.Days[0].Summary.Title);
		equal(vm.summaryTimeSpan(), fakeScheduleData.Days[0].Summary.TimeSpan);
		equal(vm.summaryStyleClassName(), fakeScheduleData.Days[0].Summary.StyleClassName);
	});

	test("should read dayoff data", function () {
		var fakeData = getFakeScheduleData();
		fakeData.Days[0].IsDayOff = true;
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeData.Days[0], week);

		equal(vm.isDayOff, true);
	});

	test("should indicate has shift", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0],  week);
		equal(vm.hasShift, true);
	});

	test("should read week day header titles", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		equal(vm.headerTitle(), "Today");
	});

	test("should read summary timespan when there is overtime and overtime availability", function () {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var vm = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);
		equal(vm.summaryTimeSpan(), fakeScheduleData.Days[0].Summary.TimeSpan);
		equal(vm.layers.length, 1);
	});
});