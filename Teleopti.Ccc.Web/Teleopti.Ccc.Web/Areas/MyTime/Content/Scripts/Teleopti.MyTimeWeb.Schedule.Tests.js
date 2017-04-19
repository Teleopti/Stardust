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

	var fakeData = {
		BaseUtcOffsetInMinutes: 60,
		Days: [{
				FixedDate: basedDate,
				Header: {
					Title: 'Some Day',
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
					EndTime: moment(basedDate).startOf('day').add('hour', 16).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
					Summary: '8:7',
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
					'EndTime': moment(basedDate).startOf('day').add('hour', 16).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
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
					Title: 'Some Day',
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
					EndTime: moment(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
					Summary: '8:7',
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
					'EndTime': moment(basedDate).startOf('day').add('day', 1).add('hour', 16).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
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
				StartTime: moment(basedDate).startOf('day').format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(basedDate).startOf('day').add('hour', 16).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 0
			}, {
				Date: moment(basedDate).add('day', 1).format('YYYY-MM-DD'),
				StartTime: moment(basedDate).add('day', 1).startOf('day').add('day', 1).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(basedDate).add('day', 1).startOf('day').add('day', 1).add('hour', 16).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 1
			}
		],
		CheckStaffingByIntraday: true,
		ViewPossibilityPermission: true,
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

	test("should apply multiple day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on ", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

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

		equal(vm.days()[1].probabilities.length, 1);
		equal(vm.days()[1].probabilities.length, 1);
		equal(vm.days()[1].probabilities[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(vm.days()[1].probabilities[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
		equal(vm.days()[1].probabilities[0].styleJson.left != '', true);
		equal(vm.days()[1].probabilities[0].styleJson.width != '', true);
	});

	//Jianfeng todo: Re-enable this test after the probability date is correct from json
	// test("should apply single day probabilities to week view model when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off ", function() {
	// 	Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
	// 		if (x === "MyTimeWeb_ViewIntradayStaffingProbability_41608") return true;
	// 		if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
	// 	};
	// 	var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeUserText, fakeAddRequestViewModel, null, null, null, undefined);
	// 	vm.probabilityType(constants.probabilityType.absence);
	// 	equal(vm.days().length, 0);

	// 	vm.initializeData(fakeData);
	// 	equal(vm.days().length, 2);
	// 	equal(vm.days()[0].probabilities.length, 1);
	// 	equal(vm.days()[0].probabilities[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
	// 	equal(vm.days()[0].probabilities[0].tooltips().indexOf(fakeUserText.probabilityForAbsence) > -1, true);
	// 	equal(vm.days()[0].probabilities[0].styleJson.left != '', true);
	// 	equal(vm.days()[0].probabilities[0].styleJson.width != '', true);
	// });
});