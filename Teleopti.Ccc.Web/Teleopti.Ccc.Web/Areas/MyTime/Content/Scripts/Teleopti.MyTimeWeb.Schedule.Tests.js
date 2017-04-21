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

	function getFakeData() {
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
			Possibilities: [
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
					Time: "06:45:00",
					TimeLineDisplay: "06:45",
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: "16:45:00",
					TimeLineDisplay: "16:45",
					PositionPercentage: 1,
					TimeFixedFormat: null
				}]
		};
	}

	test("should read absence report permission", function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		vm.initializeData(getFakeData());

		equal(vm.absenceReportPermission(), true);
	});

	test("should read scheduled days", function() {
		var fakeData = getFakeData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		vm.initializeData(fakeData);
		equal(vm.days().length, 2);
		equal(vm.days()[0].headerTitle(), fakeData.Days[0].Header.Title);
		equal(vm.days()[0].headerDayDescription(), fakeData.Days[0].Header.DayDescription);
		equal(vm.days()[0].summary(), fakeData.Days[0].Summary.Summary);
		equal(vm.days()[0].summary(), fakeData.Days[0].Summary.Summary);
		equal(vm.days()[0].summaryTitle(), fakeData.Days[0].Summary.Title);
		equal(vm.days()[0].summaryTimeSpan(), fakeData.Days[0].Summary.TimeSpan);
		equal(vm.days()[0].summaryStyleClassName(), fakeData.Days[0].Summary.StyleClassName);
		equal(vm.days()[0].hasShift, true);
		equal(vm.days()[0].noteMessage().indexOf(fakeData.Days[0].Note.Message) > -1, true);
		equal(vm.days()[0].seatBookings(), fakeData.Days[0].SeatBookings);
	});

	test("should read timelines", function() {
		var fakeData = getFakeData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeData.TimeLine = [{
			Time: "09:15:00",
			TimeLineDisplay: "09:15",
			TimeFixedFormat: null
		}];
		for (var i = 10; i <= 17; i++) {
			fakeData.TimeLine.push({
				Time: i + ":00:00",
				TimeLineDisplay: i + ":00",
				TimeFixedFormat: null
			});
		}
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);

		vm.initializeData(fakeData);

		var timelines = vm.timeLines();
		equal(timelines.length, 9);
		equal(timelines[0].minutes, 9.5 * 60 - 15); //9:30 => 9:15
		equal(timelines[timelines.length - 1].minutes, 16.75 * 60 + 15); //16:45 => 17:00
	});


	test("should apply multiple day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		vm.probabilityType(constants.probabilityType.absence);
		equal(vm.days().length, 0);

		vm.initializeData(getFakeData());
		equal(vm.days().length, 2);
		equal(vm.days()[0].probabilities.length, 1);
		equal(vm.days()[0].probabilities[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(vm.days()[0].probabilities[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[0].probabilities[0].styleJson.left != '', true);
		equal(vm.days()[0].probabilities[0].styleJson.width != '', true);

		equal(vm.days()[1].probabilities.length, 1);
		equal(vm.days()[1].probabilities.length, 1);
		equal(vm.days()[1].probabilities[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(vm.days()[1].probabilities[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[1].probabilities[0].styleJson.left != '', true);
		equal(vm.days()[1].probabilities[0].styleJson.width != '', true);
	});

	test("should apply single day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeData = getFakeData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
		vm.probabilityType(constants.probabilityType.absence);
		equal(vm.days().length, 0);

		vm.initializeData(fakeData);
		equal(vm.days().length, 2);
		equal(vm.days()[0].probabilities.length, 1);
		equal(vm.days()[0].probabilities[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(vm.days()[0].probabilities[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[0].probabilities[0].styleJson.left != '', true);
		equal(vm.days()[0].probabilities[0].styleJson.width != '', true);
		equal(vm.days()[1].probabilities.length, 0);
	});
});