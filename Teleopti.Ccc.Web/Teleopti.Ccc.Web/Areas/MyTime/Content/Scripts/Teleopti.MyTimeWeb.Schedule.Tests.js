/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js" />

$(document).ready(function() {
	module("Teleopti.MyTimeWeb.Schedule");

	var constants = Teleopti.MyTimeWeb.Common.Constants;
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

	function getFakeScheduleData() {
		return {
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: basedDate,
					Header: {
						Title: 'Today',
						Date: basedDate,
						DayDescription: '',
						DayNumber: '18'
					},
					Note: {
						Message: 'Note 1'
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
				},
				{
					FixedDate: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
					Header: {
						Title: 'Tomorrow',
						Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
						DayDescription: '',
						DayNumber: '18'
					},
					Note: {
						Message: 'Note2'
					},
					Summary: {
						Title: 'Early',
						TimeSpan: '09:30 - 16:45',
						StartTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
						'StartTime': moment(basedDate).startOf('day').add('day', 1).add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						'EndTime': moment(basedDate).startOf('day').add('day', 1).add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
				AbsenceReportPermission: true,
				AbsenceRequestPermission: true,
				OvertimeAvailabilityPermission: true,
				PersonAccountPermission: true,
				ShiftExchangePermission: true,
				ShiftTradeBulletinBoardPermission: true,
				ShiftTradeRequestPermission: false,
				TextRequestPermission: true,
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
				}]
		};
	}

	function getFakeProbabilityData(){
		return [
			{
				Date: basedDate,
				StartTime: moment(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 0
			}, {
				Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
				StartTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 1
			}
		];
	}

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate){
		var result = [];
		for (var i = 0; i < 24 * 60 / constants.probabilityIntervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate, 
				StartTime: moment(formattedDate).startOf('day').add(constants.probabilityIntervalLengthInMinute * i, "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(formattedDate).startOf('day').add(constants.probabilityIntervalLengthInMinute * (i + 1), "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: constants.probabilityIntervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	test("should read absence report permission", function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		vm.initializeData(getFakeScheduleData());

		equal(vm.absenceReportPermission(), true);
	});

	test("should read scheduled days", function() {
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		vm.initializeData(fakeScheduleData);
		equal(vm.days().length, 2);
		equal(vm.days()[0].headerTitle(), fakeScheduleData.Days[0].Header.Title);
		equal(vm.days()[0].headerDayDescription(), fakeScheduleData.Days[0].Header.DayDescription);
		equal(vm.days()[0].summary(), fakeScheduleData.Days[0].Summary.Summary);
		equal(vm.days()[0].summary(), fakeScheduleData.Days[0].Summary.Summary);
		equal(vm.days()[0].summaryTitle(), fakeScheduleData.Days[0].Summary.Title);
		equal(vm.days()[0].summaryTimeSpan(), fakeScheduleData.Days[0].Summary.TimeSpan);
		equal(vm.days()[0].summaryStyleClassName(), fakeScheduleData.Days[0].Summary.StyleClassName);
		equal(vm.days()[0].hasShift, true);
		equal(vm.days()[0].noteMessage().indexOf(fakeScheduleData.Days[0].Note.Message) > -1, true);
		equal(vm.days()[0].seatBookings(), fakeScheduleData.Days[0].SeatBookings);
	});

	test("should read timelines", function() {
		var fakeScheduleData = getFakeScheduleData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeScheduleData.TimeLine = [{
			Time: "09:15:00",
			TimeLineDisplay: "09:15",
			TimeFixedFormat: null
		}];
		for (var i = 10; i <= 17; i++) {
			fakeScheduleData.TimeLine.push({
				Time: i + ":00:00",
				TimeLineDisplay: i + ":00",
				TimeFixedFormat: null
			});
		}
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		vm.initializeData(fakeScheduleData);

		var timelines = vm.timeLines();
		equal(timelines.length, 9);
		equal(timelines[0].minutes, 9.5 * 60 - 15); //9:30 => 9:15
		equal(timelines[timelines.length - 1].minutes, 16.75 * 60 + 15); //16:45 => 17:00
	});

	test("should show no absence possibility if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return false;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence)
		week.updateProbabilityData(getFakeProbabilityData());

		week.days().forEach(function(day){
			equal(day.probabilities().length, 0);
		});
	});

	test("should show no overtime possibility if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return false;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days().forEach(function(day){
			equal(day.probabilities().length, 0);
		});
	});

	test("should show no absence possibility if set to hide probability", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.none);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days().forEach(function(day){
			equal(day.probabilities().length, 0);
		});
	});

	test("should show absence possibility within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		// Total 9:30 ~ 16:45 = 29 intervals
		equal(week.days()[0].probabilities().length, 29);
		equal(week.days()[1].probabilities().length, 0);
		for (var i = 0; i < week.days()[0].probabilities.length; i++) {
			if (i > 0) {
				equal(week.days()[0].probabilities[i].tooltips().length > 0, true);
			}
		}
	});

	test("should show overtime possibility within timeline range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [{
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
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 96);
		equal(week.days()[1].probabilities().length, 0);
		for (var i = 0; i < week.days()[0].probabilities.length; i++) {
			equal(week.days()[0].probabilities[i].tooltips().length > 0, true);
		}
	});

	test("should hide absence possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(750); // 12:30

		equal(week.days()[0].probabilities().length, 29);

		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			// Schedule started from 09:30, current time is 12:30
			// Then the first (09:30 - 12:30) * 4 = 12 probabilities should be masked
			if (i < 12)
				equal(week.days()[0].probabilities()[i].tooltips().length > 0, false);
			else
				equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should hide overtime possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(750); // 12:30

		equal(week.days()[0].probabilities().length, 29);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			var probability = week.days()[0].probabilities()[i];
			//9:30 ~ 12:30 = 3 * 4 intervals
			if (i < 12) {
				equal(probability.tooltips().length, 0);
				equal(probability.cssClass().indexOf(constants.probabilityClass.expiredProbabilityClass) > -1, true);
			} else {
				equal(probability.cssClass().indexOf(constants.probabilityClass.expiredProbabilityClass), -1);
				equal(probability.tooltips().length > 0, true);
			}
		}
	});

	test("should show no absence possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);
		equal(week.days()[0].probabilities().length, 0);
	});

	test("should show no absence possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 0);
	});

	test("should show overtime possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.TimeLine = [{
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
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);
		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(week.days()[0].probabilities().length, 38);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		// In this scenario will show prabability based on length of intraday open hour
		// So should be (15 - 10) * 4
		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal( week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show overtime possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		fakeScheduleData.TimeLine = [{
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
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(week.days()[0].probabilities().length, 38);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [{
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
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//18 * 4 - 1 margin = 71
		equal(week.days()[0].probabilities().length, 71);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//according to scheduel period 00:00 ~ 16:45 = 16.75 * 4 = 
		equal(week.days()[0].probabilities().length, 67);

		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [{
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

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//9:30 ~ 24:00 = 14.5 * 4 = 58
		equal(week.days()[0].probabilities().length, 58);

	});

	test("should apply multiple day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		equal(vm.days().length, 0);
		vm.initializeData(getFakeScheduleData());

		vm.selectedProbabilityType(constants.probabilityType.absence);
		vm.updateProbabilityData(getFakeProbabilityData());

		equal(vm.days().length, 2);
		equal(vm.days()[0].probabilities().length, 1);
		equal(vm.days()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(vm.days()[0].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[0].probabilities()[0].styleJson.left != '', true);
		equal(vm.days()[0].probabilities()[0].styleJson.width != '', true);

		equal(vm.days()[1].probabilities().length, 1);
		equal(vm.days()[1].probabilities().length, 1);
		equal(vm.days()[1].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(vm.days()[1].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[1].probabilities()[0].styleJson.left != '', true);
		equal(vm.days()[1].probabilities()[0].styleJson.width != '', true);
	});

	test("should apply single day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		equal(vm.days().length, 0);
		vm.initializeData(fakeScheduleData);

		vm.selectedProbabilityType(constants.probabilityType.absence);
		vm.updateProbabilityData(getFakeProbabilityData());
		equal(vm.days().length, 2);
		equal(vm.days()[0].probabilities().length, 1);
		equal(vm.days()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(vm.days()[0].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[0].probabilities()[0].styleJson.left != '', true);
		equal(vm.days()[0].probabilities()[0].styleJson.width != '', true);
		equal(vm.days()[1].probabilities().length, 0);
	});

	test("should not show probability toggle if current week doesn't intercept with 14 upcoming days period even when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeScheduleData = getFakeScheduleData();

		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		vm.initializeData(fakeScheduleData);

		vm.selectedProbabilityType(constants.probabilityType.absence);
		vm.updateProbabilityData(getFakeProbabilityData());
		equal(vm.showProbabilityToggle(), true);

		fakeScheduleData.Days[0].FixedDate = moment(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		fakeScheduleData.Days[0].FixedDate = moment(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeScheduleData.Days[1].Periods[0].StartTime = moment(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[1].Periods[0].EndTime = moment(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		vm.selectedProbabilityType(constants.probabilityType.none);
		vm.initializeData(fakeScheduleData);
		equal(vm.showProbabilityToggle(), false);
	});
});
