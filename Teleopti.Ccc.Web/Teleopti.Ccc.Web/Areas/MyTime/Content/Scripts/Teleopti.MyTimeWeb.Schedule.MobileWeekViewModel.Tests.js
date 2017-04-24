/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");
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

	var basedDate = moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD');

	function getFakeScheduleData(){
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
				},
				{
					FixedDate: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
					IsDayOff: false,
					IsFullDayAbsence: false,
					Header: {
						Title: 'Tomorrow',
						Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
						DayDescription: '',
						DayNumber: '18'
					},
					Note: {
						Message: ''
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
						'StartTime': moment(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						'EndTime': moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
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

	test("should read absence report permission", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);

		weekViewModel.readData({
			PeriodSelection: [{ Display: null }],
			Days: [{}],
			RequestPermission:
			{
				AbsenceReportPermission: true
			}
		});

		equal(weekViewModel.absenceReportPermission(), true);
		equal(weekViewModel.dayViewModels()[0].absenceReportPermission(), true);
	});

	test("should read scheduled days", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);

		weekViewModel.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
			}]
		});

		equal(weekViewModel.dayViewModels().length, 1);
	});

	test("should read timelines", function () {
		var fakeScheduleData = getFakeScheduleData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeScheduleData.TimeLine = [{
			Time: "09:15:00",
			TimeLineDisplay: "09:15",
			TimeFixedFormat: null
		}];
		for(var i = 10; i <= 17; i++){
			fakeScheduleData.TimeLine.push({
				Time: i + ":00:00",
				TimeLineDisplay: i + ":00",
				TimeFixedFormat: null
			});
		}
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var rawData = {
			PeriodSelection: [{
				Display: null
			}],
			Days: [{}],
			TimeLine: fakeScheduleData.TimeLine
		};

		weekViewModel.readData(rawData);

		var timelines = weekViewModel.timeLines();
		equal(timelines.length, 9);
		equal(timelines[0].minutes, 9.5 * 60 - 15);		//9:30 => 9:15
		equal(timelines[timelines.length - 1].minutes, 16.75 * 60 + 15); 	//16:45 => 17:00
	});
	//-128
	test("should show no absence probability if the feature is disabled", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return false;
		};

		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);

		equal(weekViewModel.selectedProbabilityOptionValue(), undefined);
		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});

	test("should show no absence probability if agent has no permission", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.ViewPossibilityPermission = false;
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});

	test("should set default probability option to hidden ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);

		equal(weekViewModel.selectedProbabilityOptionValue(), undefined);
	});

	test("should show no absence probability if set to hide probability", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.none);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});

	test("should show absence probability within schedule time range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show overtime probability within timeline range", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine[1].Time = "15:00:00";		//narrow timeline to 9:30 - 15:00
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);

		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 14:45") > -1, true);
	});

	test("should show no absence probability for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.Days[0].Periods = [];

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});

	test("should show no absence probability for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});

	test("should show overtime probability for dayoff", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.Days[0].Periods = [];
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show overtime probability for fullday absence", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show overtime probability based on intraday open hour", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.SiteOpenHourIntradayPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("10:00 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 15:00") > -1, true);
	});

	test("should show correct overtime possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [{
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
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');
		
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("00:00 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show correct absence possibility for cross day schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [{
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
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(basedDate).subtract('day', 1).add('hour', 22).format('YYYY-MM-DDTHH:mm:ss');

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("00:00 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 16:45") > -1, true);
	});

	test("should show absence possibility for night shift schedule", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.dayViewModels()[0].userNowInMinute(0);

		// Will generate probabilities from schedule start (10:00) to schedule end (00:00+)
		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf("09:30 - 12:00") > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf("12:00 - 00:00 +1") > -1, true);
	});

	test("should change probability option value to absence(1) after selecting 'Show absence probability' ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');

		var ajax = {
			Ajax: function(data){
				if(data.url == '../api/ScheduleStaffingPossibility' && data.success)
					data.success(fakeProbabilityData);
			}
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, ajax, null);
		weekViewModel.readData(fakeScheduleData);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.toggleProbabilityOptionsPanel();

		equal(weekViewModel.requestViewModel().model.checkedProbability(), undefined);
		weekViewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.absence);
		equal(weekViewModel.selectedProbabilityOptionValue(), constants.probabilityType.absence);
	});

	test("should change staffing probability option value to overtime(2) after selecting 'Show overtime  probability' ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');

		var ajax = {
			Ajax: function(data){
				if(data.url == '../api/ScheduleStaffingPossibility' && data.success)
					data.success(fakeProbabilityData);
			}
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, ajax, null);
		weekViewModel.readData(fakeScheduleData);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.toggleProbabilityOptionsPanel();

		equal(weekViewModel.requestViewModel().model.checkedProbability(), undefined);
		weekViewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.overtime);
		equal(weekViewModel.selectedProbabilityOptionValue(), constants.probabilityType.overtime);
	});

	test("should toggle off staffing probability after selecting Hide staffing probability ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		var ajax = {
			Ajax: function(data){
				if(data.url == '../api/ScheduleStaffingPossibility' && data.success)
					data.success(fakeProbabilityData);
			}
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, ajax, null);
		weekViewModel.readData(fakeScheduleData);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(weekViewModel.dayViewModels()[0].fixedDate());
		weekViewModel.updateProbabilityData(fakeProbabilityData);
		weekViewModel.toggleProbabilityOptionsPanel();

		equal(weekViewModel.requestViewModel().model.checkedProbability(), undefined);
		weekViewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.none);
		equal(weekViewModel.requestViewModel(), undefined);
		equal(weekViewModel.selectedProbabilityOptionValue(), constants.probabilityType.none);
	});

	test("Should show the staffing probability for today", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};
		
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		var fakeScheduleData = getFakeScheduleData();

		weekViewModel.readData(fakeScheduleData);
		equal(weekViewModel.dayViewModels()[0].showProbabilityToggleIcon(), true);
	});

	test("Should not show probability option model if it is not current day when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);

		var fakeFixedDateObj = {fixedDate: function(){return fakeScheduleData.Days[0].FixedDate}};
		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.dayViewModels()[0].showProbabilityToggleIcon(), true);
		equal(weekViewModel.requestViewModel() != null, true);
		equal(weekViewModel.dayViewModels()[0].isModelVisible(), true);

		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.requestViewModel() == null, true);
		equal(weekViewModel.dayViewModels()[1].showProbabilityToggleIcon(), false);
		equal(weekViewModel.dayViewModels()[1].isModelVisible(), false);
	});

	test("Should toggle the global staffing probability option form after clicking the toggle icon when 'MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880' is on", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		var fakeFixedDateObj = {fixedDate: function(){return fakeScheduleData.Days[0].FixedDate}};

		equal(weekViewModel.dayViewModels()[0].showProbabilityToggleIcon(), false);
		equal(weekViewModel.dayViewModels()[1].showProbabilityToggleIcon(), false);
		equal(weekViewModel.showProbabilityOptionsToggleIcon(), true);
		equal(weekViewModel.showProbabilityOptionsForm(), false);

		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.showProbabilityOptionsForm(), true);

		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.showProbabilityOptionsForm(), false);
	});

	test("Should show probability data for multiple upcoming days when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};
		var fakeScheduleData = getFakeScheduleData();
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(basedDate);
		fakeProbabilityData.push({
			Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
			StartTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 15).format('YYYY-MM-DDTHH:mm:ss'),
			EndTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
			Possibility: 1
		});

		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeScheduleData);
		//Steps in production code:
		//var fakeFixedDateObj = {fixedDate: function(){return fakeScheduleData.Days[0].FixedDate}};
		//weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		//weekViewModel.OnProbabilityOptionSelectCallback(Teleopti.MyTimeWeb.Common.Constants.probabilityType.overtime);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime)
		weekViewModel.updateProbabilityData(fakeProbabilityData);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf('09:30 - 12:00') > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[1].tooltips().indexOf('12:00 - 16:45') > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].styleJson.left != '', true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].styleJson.width != '', true);

		equal(weekViewModel.dayViewModels()[1].probabilities().length, 1);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].tooltips().indexOf('09:30 - 16:45') > -1, true);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].styleJson.left != '', true);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].styleJson.width != '', true);
	});

	test("should not show probability toggle if current week doesn't intercept with 14 upcoming days period even when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeScheduleData = getFakeScheduleData();
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(basedDate);
		var ajax = {
			Ajax: function(data){
				if(data.url == '../api/ScheduleStaffingPossibility' && data.success)
					data.success(fakeProbabilityData);
			}
		};
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, ajax, null);
		weekViewModel.readData(fakeScheduleData);
		//Steps in production code:
		//var fakeFixedDateObj = {fixedDate: function(){return fakeScheduleData.Days[0].FixedDate}};
		//weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		//weekViewModel.OnProbabilityOptionSelectCallback(Teleopti.MyTimeWeb.Common.Constants.probabilityType.overtime);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime)
		weekViewModel.updateProbabilityData(fakeProbabilityData);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 2);

		//change the fakeScheduleData and make period of it exceeds 14 upcoming days 
		fakeScheduleData.Days[0].FixedDate = moment(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(fakeScheduleData.Days[0].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		fakeScheduleData.Days[0].FixedDate = moment(basedDate).add('day', 16).format('YYYY-MM-DD');
		fakeScheduleData.Days[1].Periods[0].StartTime = moment(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[1].Periods[0].EndTime = moment(fakeScheduleData.Days[1].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		weekViewModel.readData(fakeScheduleData);
		equal(weekViewModel.showProbabilityOptionsToggleIcon(), false);
		equal(weekViewModel.dayViewModels()[0].probabilities().length, 0);
	});
});