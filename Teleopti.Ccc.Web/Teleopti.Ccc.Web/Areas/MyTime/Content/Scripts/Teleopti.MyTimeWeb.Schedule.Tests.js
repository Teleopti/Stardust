/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/hasher/hasher.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js" />

$(document).ready(function() {
	module("Teleopti.MyTimeWeb.Schedule");

	var hash = "";

	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var fakeAddRequestViewModel = function() {
		return {
			DateFormat: function() {
				return 'YYYY-MM-DD';
			}
		};
	};
	var momentWithLocale = function(date){return moment(date).locale('en-gb');};
	var basedDate = momentWithLocale(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD'); 

	function getFakeScheduleData() {
		return {
			PeriodSelection: {
				StartDate: momentWithLocale(basedDate).weekday(0).format('YYYY-MM-DDTHH:mm:ss'),
				EndDate: momentWithLocale(basedDate).weekday(6).format('YYYY-MM-DDTHH:mm:ss'),
				Display: momentWithLocale(basedDate).weekday(0).format('YYYY/MM/DDTHH:mm:ss') + ' - ' + momentWithLocale(basedDate).weekday(6).format('YYYY/MM/DDTHH:mm:ss'),
				Date: basedDate,
				SelectedDateRange: {
					MinDate: momentWithLocale(basedDate).weekday(0).format('YYYY-MM-DD'),
					MaxDate: momentWithLocale(basedDate).weekday(6).format('YYYY-MM-DD')
				},
				SelectableDateRange: {
					MinDate: "1900-04-30",
					MaxDate: "2077-11-16"
				},
				PeriodNavigation: {
					NextPeriod: momentWithLocale(basedDate).add('day', 7).format('YYYY-MM-DD'),
					HasNextPeriod: true,
					PrevPeriod: momentWithLocale(basedDate).subtract('day', 7).format('YYYY-MM-DD'),
					HasPrevPeriod: true,
					CanPickPeriod: false
				}
			},
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
						StartTime: momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
						'StartTime': momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						'EndTime': momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
					FixedDate: momentWithLocale(basedDate).add('day', 1).format('YYYY-MM-DD'),
					Header: {
						Title: 'Tomorrow',
						Date: momentWithLocale(basedDate).add('day', 1).format('YYYY-MM-DD'),
						DayDescription: '',
						DayNumber: '18'
					},
					Note: {
						Message: 'Note2'
					},
					Summary: {
						Title: 'Early',
						TimeSpan: '09:30 - 16:45',
						StartTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
						'StartTime': momentWithLocale(basedDate).startOf('day').add('day', 1).add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						'EndTime': momentWithLocale(basedDate).startOf('day').add('day', 1).add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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
			IsCurrentWeek: true,
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
				StartTime: momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 0
			}, {
				Date: momentWithLocale(basedDate).add('day', 1).format('YYYY-MM-DD'),
				StartTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 1
			}
		];
	}

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate){
		var result = [];
		for (var i = 0; i < 24 * 60 / constants.probabilityIntervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate, 
				StartTime: momentWithLocale(formattedDate).startOf('day').add(constants.probabilityIntervalLengthInMinute * i, "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(formattedDate).startOf('day').add(constants.probabilityIntervalLengthInMinute * (i + 1), "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: constants.probabilityIntervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	var userTexts = {
		HideStaffingInfo: "HideStaffingInfo",
		XRequests: "XRequests",
		Fair: "Fair",
		ShowAbsenceProbability: "ShowAbsenceProbability",
		ProbabilityToGetAbsenceColon: "ProbabilityToGetAbsenceColon",
		ProbabilityToGetOvertimeColon: "ProbabilityToGetOvertimeColon",
		StaffingInfo: "StaffingInfo",
		ShowOvertimeProbability: "ShowOvertimeProbability"
	}

	function initUserTexts() {
	    Teleopti.MyTimeWeb.Common.SetUserTexts(userTexts);
	}

	test("should read absence report permission", function () {
		initUserTexts();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.absenceReportPermission(), true);
	});

	test("should read scheduled days", function() {
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

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

	test("should read timelines", function () {
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
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

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
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
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
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
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
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.none;

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days().forEach(function(day){
			equal(day.probabilities().length, 0);
		});
	});

	test("should keep possibility selection for today when changing date", function () {
		initUserTexts();

		setupHash();

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		week.nextWeek();

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.previousWeek();

		equal(week.selectedProbabilityType, constants.probabilityType.absence); 

		equal(Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).probability, constants.probabilityType.absence);

		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});  

	test("should keep possibility selection for multiple days when changing date", function () {
		initUserTexts();
		setupHash();

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);


		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.previousWeek();

		equal(week.selectedProbabilityType, constants.probabilityType.overtime);
		equal(Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).probability, constants.probabilityType.overtime);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show absence possibility within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;

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
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
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
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 96);
		equal(week.days()[1].probabilities().length, 0);
		for (var i = 0; i < week.days()[0].probabilities.length; i++) {
			equal(week.days()[0].probabilities[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should hide absence possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
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
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should hide overtime possibility earlier than now", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
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
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show no absence possibility for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);
		equal(week.days()[0].probabilities().length, 0);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show no absence possibility for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 0);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
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
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);
		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(week.days()[0].probabilities().length, 38);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show overtime possibility based on site open hour", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].SiteOpenHourPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show overtime possibility for dayoff based on intraday open hour", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.Days[0].SiteOpenHourPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		// In this scenario will show prabability based on length of intraday open hour
		// So should be (15 - 10) * 4
		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal( week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
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

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
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
		fakeScheduleData.Days[0].StartTime = momentWithLocale(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
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

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//18 * 4 - 1 margin = 71
		equal(week.days()[0].probabilities().length, 71);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].StartTime = momentWithLocale(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//according to scheduel period 00:00 ~ 16:45 = 16.75 * 4 = 
		equal(week.days()[0].probabilities().length, 67);

		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(week.days()[0].probabilities()[i].tooltips().length > 0, true);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = momentWithLocale(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
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

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate());
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute(0);

		//9:30 ~ 24:00 = 14.5 * 4 = 58
		equal(week.days()[0].probabilities().length, 58);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should apply multiple day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		equal(week.days().length, 0);
		week.initializeData(getFakeScheduleData());

		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());

		equal(week.days().length, 2);
		equal(week.days()[0].probabilities().length, 1);
		equal(week.days()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(week.days()[0].probabilities()[0].tooltips().indexOf(userTexts.ProbabilityToGetAbsenceColon) > -1, true);
		equal(week.days()[0].probabilities()[0].styleJson.left != '', true);
		equal(week.days()[0].probabilities()[0].styleJson.width != '', true);

		equal(week.days()[1].probabilities().length, 1);
		equal(week.days()[1].probabilities().length, 1);
		equal(week.days()[1].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(week.days()[1].probabilities()[0].tooltips().indexOf(userTexts.ProbabilityToGetAbsenceColon) > -1, true);
		equal(week.days()[1].probabilities()[0].styleJson.left != '', true);
		equal(week.days()[1].probabilities()[0].styleJson.width != '', true);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should apply single day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off ", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		equal(week.days().length, 0);
		week.initializeData(fakeScheduleData);

		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());
		equal(week.days().length, 2);
		equal(week.days()[0].probabilities().length, 1);
		equal(week.days()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(week.days()[0].probabilities()[0].tooltips().indexOf(userTexts.ProbabilityToGetAbsenceColon) > -1, true);
		equal(week.days()[0].probabilities()[0].styleJson.left != '', true);
		equal(week.days()[0].probabilities()[0].styleJson.width != '', true);
		equal(week.days()[1].probabilities().length, 0);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should not show probability toggle if current week doesn't intercept with 14 upcoming days period even when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeScheduleData = getFakeScheduleData();

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);

		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());
		equal(week.showProbabilityToggle(), true);

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeScheduleData.Days[0].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[0].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeScheduleData.Days[1].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[1].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		week.selectedProbabilityType = constants.probabilityType.none;
		week.initializeData(fakeScheduleData);
		equal(week.showProbabilityToggle(), false);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should select absence probability option when switching to absence probability type", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
			return false;
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === "../api/ScheduleStaffingPossibility") {
					options.success(getFakeProbabilityData());
				}
			}
		}

		$("body").append("<span data-bind='text: probabilityLabel()' class='probabilityLabel'></span>");

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {},
			function() {},
			ajax
		);

		var fakeScheduleData = getFakeScheduleData();

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData); 
	  
		week.switchProbabilityType(constants.probabilityType.absence);

		equal(userTexts.ShowAbsenceProbability, $(".probabilityLabel").text());

		$(".probabilityLabel").remove();
	});

	test("should select hide staffing info option when switching to hide probability", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
			return false;
		};

		var ajax = {
			Ajax: function (options) {
				if (options.url === "../api/ScheduleStaffingPossibility") {
					options.success(getFakeProbabilityData());
				}
			}
		}

		$("body").append("<span data-bind='text: probabilityLabel()' class='probabilityLabel'></span>");

		Teleopti.MyTimeWeb.Schedule.PartialInit(function () { },
			function () { },
			ajax
		);

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData); 

		week.switchProbabilityType(constants.probabilityType.absence);
		week.switchProbabilityType(constants.probabilityType.none);
		equal(userTexts.HideStaffingInfo, $(".probabilityLabel").text());

		$(".probabilityLabel").remove();
	});

	test("should select hide staffing info option when CheckStaffingByIntraday is changed to false from true", function () {

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
			return false;
		};

		var ajax = {
			Ajax: function (options) {
				if (options.url === "../api/ScheduleStaffingPossibility") {
					options.success(getFakeProbabilityData());
				}
			}
		}

		$("body").append("<span data-bind='text: probabilityLabel()' class='probabilityLabel'></span>");

		Teleopti.MyTimeWeb.Schedule.PartialInit(function () { },
			function () { },
			ajax
		);

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData); 

		week.switchProbabilityType(constants.probabilityType.absence);

		fakeScheduleData.CheckStaffingByIntraday = false;

		week.initializeData(fakeScheduleData); 

		equal(userTexts.HideStaffingInfo, $(".probabilityLabel").text());

		$(".probabilityLabel").remove();
	});

	test("should open new start page when changing to mobile view and  toggle 43446 is on", function () {
		initUserTexts();
		setupHash();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);

		vm.mobile(); 

		equal("#Schedule/MobileDay", hash);
	});

	test("should reload schedule when switch to overtime probability", function () {
		initUserTexts();

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
			return false;
		};

		var invokeFetchWeekDataCount = 0;
		var ajax = {
			Ajax: function (options) {
				if (options.url === "../api/ScheduleStaffingPossibility") {
					options.success(getFakeProbabilityData());
				}
				if (options.url === "../api/Schedule/FetchWeekData") {
					invokeFetchWeekDataCount++;
					options.success(getFakeScheduleData());
				}
			}
		}

		Teleopti.MyTimeWeb.Schedule.PartialInit(function () { },
			function () { },
			ajax
		); 

		Teleopti.MyTimeWeb.Request.RequestDetail = {
			AddTextOrAbsenceRequest: function () { 
			}
		};

		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function (whenLoadedCallBack) {
				var data = { WeekStart: "" };
				whenLoadedCallBack(data);
			}
		};

		Teleopti.MyTimeWeb.Schedule.SetupViewModel(undefined, function() {}); 

		var fakeScheduleData = getFakeScheduleData();

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);  
		week.switchProbabilityType(constants.probabilityType.overtime);

		equal(1, invokeFetchWeekDataCount);
	});

	function setupHash() {
		this.hasher = {
			initialized: {
				add: function () { }
			},
			changed: {
				add: function () { }
			},
			init: function () { },
			setHash: function (data) { hash = "#"+data; }
		};
	}
});
