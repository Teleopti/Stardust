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

	var stopFetchProbabilityData = true;

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
			};
	}

	function fakeDataProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate){
		var result = [];
		for (var i = 0; i < 24 * 60 / intervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate,
				StartTime: moment(formattedDate).startOf('day').add(intervalLengthInMinute * i, "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(formattedDate).startOf('day').add(intervalLengthInMinute * (i + 1), "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: intervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	test("should read date", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeScheduleData().Days[0], false, true, weekViewModel);

		equal(vm.fixedDate(), basedDate);
		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should read permission", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel({}, false, true, weekViewModel);

		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should load shift category data", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeScheduleData().Days[0], true, true, weekViewModel);
		equal(vm.summaryName(), getFakeScheduleData().Days[0].Summary.Title);
		equal(vm.summaryTimeSpan(), getFakeScheduleData().Days[0].Summary.TimeSpan);
		equal(vm.summaryColor(), getFakeScheduleData().Days[0].Summary.Color);
		equal(vm.summaryStyleClassName(), getFakeScheduleData().Days[0].Summary.StyleClassName);
		equal(vm.backgroundColor, getFakeScheduleData().Days[0].Summary.Color);
	});

	test("should read dayoff data", function () {
		var fakeData = getFakeScheduleData();
		fakeData.Days[0].IsDayOff = true;
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(fakeData.Days[0], true, true, weekViewModel);

		equal(vm.isDayoff(), true);
	});

	test("should indicate has shift", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeScheduleData().Days[0], true, true, weekViewModel);
		equal(vm.hasShift, true);
	});

	test("should read week day header titles", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeScheduleData().Days[0], true, true, weekViewModel);

		equal(vm.weekDayHeaderTitle(), "Today");
	});

	test("should read summary timespan when there is overtime and overtime availability", function () {
		var weekViewModel = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, null, stopFetchProbabilityData);
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(getFakeScheduleData().Days[0], true, true, weekViewModel);
		equal(vm.summaryTimeSpan(), getFakeScheduleData().Days[0].Summary.TimeSpan);
		equal(vm.layers.length, 1);
	});
});