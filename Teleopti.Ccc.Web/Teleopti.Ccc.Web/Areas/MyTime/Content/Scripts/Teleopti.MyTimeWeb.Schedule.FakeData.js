/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/hasher/hasher.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js" />

Teleopti.MyTimeWeb.Schedule.FakeData = (function ($) {
	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var fakeAddRequestViewModel = function () {
		return {
			DateFormat: function () {
				return 'YYYY-MM-DD';
			}
		};
	};
	var momentWithLocale = function (date) { return moment(date).locale('en-gb'); };
	var basedDate = momentWithLocale(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes))
		.format('YYYY-MM-DD');

	function getFakeScheduleData() {
		return {
			PeriodSelection: {
				StartDate: momentWithLocale(basedDate).weekday(0).format('YYYY-MM-DDTHH:mm:ss'),
				EndDate: momentWithLocale(basedDate).weekday(6).format('YYYY-MM-DDTHH:mm:ss'),
				Display: momentWithLocale(basedDate).weekday(0).format('YYYY/MM/DDTHH:mm:ss') +
				' - ' +
				momentWithLocale(basedDate).weekday(6).format('YYYY/MM/DDTHH:mm:ss'),
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
			Days: [
				{
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
						StartTime: momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45)
							.format('YYYY-MM-DDTHH:mm:ss'),
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
					Periods: [
						{
							'Title': 'Phone',
							'TimeSpan': '09:30 - 16:45',
							'StartTime': momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30)
								.format('YYYY-MM-DDTHH:mm:ss'),
							'EndTime': momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45)
								.format('YYYY-MM-DDTHH:mm:ss'),
							'Summary': '7:15',
							'StyleClassName': 'color_80FF80',
							'Meeting': null,
							'StartPositionPercentage': 0.1896551724137931034482758621,
							'EndPositionPercentage': 1,
							'Color': '128,255,128',
							'IsOvertime': false
						}
					]
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
						StartTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45)
							.format('YYYY-MM-DDTHH:mm:ss'),
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
					Periods: [
						{
							'Title': 'Phone',
							'TimeSpan': '09:30 - 16:45',
							'StartTime': momentWithLocale(basedDate).startOf('day').add('day', 1).add('hour', 9).add('minute', 30)
								.format('YYYY-MM-DDTHH:mm:ss'),
							'EndTime': momentWithLocale(basedDate).startOf('day').add('day', 1).add('hour', 16).add('minute', 45)
								.format('YYYY-MM-DDTHH:mm:ss'),
							'Summary': '7:15',
							'StyleClassName': 'color_80FF80',
							'Meeting': null,
							'StartPositionPercentage': 0.1896551724137931034482758621,
							'EndPositionPercentage': 1,
							'Color': '128,255,128',
							'IsOvertime': false
						}
					]
				}
			],
			IsCurrentWeek: true,
			AbsenceProbabilityEnabled: true,
			CheckStaffingByIntraday: true,
			OvertimeProbabilityEnabled: true,
			ViewPossibilityPermission: true,
			RequestPermission: {
				AbsenceReportPermission: true,
				AbsenceRequestPermission: true,
				OvertimeRequestPermission: true,
				OvertimeAvailabilityPermission: true,
				PersonAccountPermission: true,
				ShiftExchangePermission: true,
				ShiftTradeBulletinBoardPermission: true,
				ShiftTradeRequestPermission: false,
				TextRequestPermission: true,
			},
			TimeLine: [
				{
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
				}
			]
		};
	}

	function getFakeProbabilityData() {
		return [
			{
				Date: basedDate,
				StartTime: momentWithLocale(basedDate).startOf('day').add('hour', 9).add('minute', 30)
					.format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(basedDate).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 0
			}, {
				Date: momentWithLocale(basedDate).add('day', 1).format('YYYY-MM-DD'),
				StartTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 9).add('minute', 30)
					.format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(basedDate).add('day', 1).startOf('day').add('hour', 16).add('minute', 45)
					.format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 1
			}
		];
	}

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate) {
		var result = [];
		for (var i = 0; i < 24 * 60 / constants.probabilityIntervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate,
				StartTime: momentWithLocale(formattedDate).startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * i, "minutes").format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: momentWithLocale(formattedDate).startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * (i + 1), "minutes").format('YYYY-MM-DDTHH:mm:ss'),
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

	return {
		getFakeScheduleData: getFakeScheduleData,
		getFakeProbabilityData: getFakeProbabilityData,
		fakeProbabilitiesDataLowBeforeTwelveAndHighAfter: fakeProbabilitiesDataLowBeforeTwelveAndHighAfter,
		userTexts: userTexts,
		fakeAddRequestViewModel: fakeAddRequestViewModel
	};
})(jQuery);
