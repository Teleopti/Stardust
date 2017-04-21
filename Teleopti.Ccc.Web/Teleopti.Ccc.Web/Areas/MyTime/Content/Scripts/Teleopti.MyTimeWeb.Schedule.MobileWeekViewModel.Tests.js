/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");
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
			Possibilities: [
				{
					Date: basedDate,
					StartTime: moment(basedDate).startOf('day').add('hour', 9).add('minute', 15).format('YYYY-MM-DDTHH:mm:ss'),
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

	test("should read absence report permission", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{ Display: null }],
			Days: [{}],
			RequestPermission:
			{
				AbsenceReportPermission: true
			}
		});

		equal(vm.absenceReportPermission(), true);
		equal(vm.dayViewModels()[0].absenceReportPermission(), true);
	});

	test("should read scheduled days", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
			}]
		});

		equal(vm.dayViewModels().length, 1);
	});

	test("should read timelines", function () {
		var fakeData = getFakeData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeData.TimeLine = [{
			Time: "09:15:00",
			TimeLineDisplay: "09:15",
			TimeFixedFormat: null
		}];
		for(var i = 10; i <= 17; i++){
			fakeData.TimeLine.push({
				Time: i + ":00:00",
				TimeLineDisplay: i + ":00",
				TimeFixedFormat: null
			});
		}
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		var rawData = {
			PeriodSelection: [{
				Display: null
			}],
			Days: [{}],
			TimeLine: fakeData.TimeLine
		};

		vm.readData(rawData);

		var timelines = vm.timeLines();
		equal(timelines.length, 9);
		equal(timelines[0].minutes, 9.5 * 60 - 15);		//9:30 => 9:15
		equal(timelines[timelines.length - 1].minutes, 16.75 * 60 + 15); 	//16:45 => 17:00
	});

	test("should set default probability option to hidden ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);

		equal(weekViewModel.selectedProbabilityOptionValue(), undefined);
	});

	test("should change probability option value to absence(1) after selecting Show absence probability ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);
		weekViewModel.toggleProbabilityOptionsPanel();

		equal(weekViewModel.requestViewModel().model.checkedProbability(), undefined);
		weekViewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.absence);
		equal(weekViewModel.selectedProbabilityOptionValue(), constants.probabilityType.absence);
	});

	test("should change staffing probability option value to overtime(2) after selecting Show overtime  probability ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);
		weekViewModel.toggleProbabilityOptionsPanel();

		equal(weekViewModel.requestViewModel().model.checkedProbability(), undefined);
		weekViewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.overtime);
		equal(weekViewModel.selectedProbabilityOptionValue(), constants.probabilityType.overtime);
	});

	test("should toggle off staffing probability after selecting Hide staffing probability ", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
		};
		var fakeData = getFakeData();
		fakeData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Possibilities = fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);
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
		var fakeData = {
			PeriodSelection: [{ Display: null }],
			BaseUtcOffsetInMinutes: 60,
			Days: [
			{
				FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD')
			}],
			ViewPossibilityPermission: true
		};

		weekViewModel.readData(fakeData);
		equal(weekViewModel.dayViewModels()[0].showProbabilityOptions(), true);
	});

	test("Should not show probability option model if it is not current day when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeData = getFakeData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};
		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.dayViewModels()[0].showProbabilityOptions(), true);
		equal(weekViewModel.requestViewModel() != null, true);
		equal(weekViewModel.dayViewModels()[0].isModelVisible(), true);

		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(weekViewModel.requestViewModel() == null, true);
		equal(weekViewModel.dayViewModels()[1].showProbabilityOptions(), false);
		equal(weekViewModel.dayViewModels()[1].isModelVisible(), false);
	});

	test("Should toggle the global staffing probability option form after clicking the toggle icon when 'MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880' is on", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeData = getFakeData();
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null);
		weekViewModel.readData(fakeData);
		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};

		equal(weekViewModel.dayViewModels()[0].showProbabilityOptions(), false);
		equal(weekViewModel.dayViewModels()[1].showProbabilityOptions(), false);
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
		var fakeData = getFakeData();
		fakeData.Possibilities.push({
			Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
			StartTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 15).format('YYYY-MM-DDTHH:mm:ss'),
			EndTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
			Possibility: 1
		});
		var weekViewModel;
		var fakeReloadData = function(){
			weekViewModel.readData(fakeData);
		};

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};
		weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, fakeReloadData);
		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		weekViewModel.OnProbabilityOptionSelectCallback(Teleopti.MyTimeWeb.Common.Constants.probabilityType.overtime);

		equal(weekViewModel.dayViewModels()[0].probabilities().length, 1);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].styleJson.left != '', true);
		equal(weekViewModel.dayViewModels()[0].probabilities()[0].styleJson.width != '', true);

		equal(weekViewModel.dayViewModels()[1].probabilities().length, 1);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].styleJson.left != '', true);
		equal(weekViewModel.dayViewModels()[1].probabilities()[0].styleJson.width != '', true);
	});

	test("should not show probability toggle if current week doesn't intercept with 14 upcoming days period even when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var weekViewModel;
		var fakeData = getFakeData();
		var fakeReloadData = function(){
			weekViewModel.readData(fakeData);
		};

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};
		weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, fakeReloadData);
		weekViewModel.readData(fakeData);
		weekViewModel.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		weekViewModel.selectedProbabilityOptionValue(constants.probabilityType.absence);
		equal(weekViewModel.showProbabilityOptionsToggleIcon(), true);

		fakeData.Days[0].FixedDate = moment(basedDate).add('day', 15).format('YYYY-MM-DD');
		fakeData.Days[0].Periods[0].StartTime = moment(fakeData.Days[0].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Days[0].Periods[0].EndTime = moment(fakeData.Days[0].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		fakeData.Days[0].FixedDate = moment(basedDate).add('day', 16).format('YYYY-MM-DD');
		fakeData.Days[1].Periods[0].StartTime = moment(fakeData.Days[1].FixedDate).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss');
		fakeData.Days[1].Periods[0].EndTime = moment(fakeData.Days[1].FixedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss');

		weekViewModel.readData(fakeData);
		equal(weekViewModel.showProbabilityOptionsToggleIcon(), false);
	});
});