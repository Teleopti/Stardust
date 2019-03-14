$(document).ready(function() {
	module("Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary");
	Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

	var constants = Teleopti.MyTimeWeb.Common.Constants;

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
					}],
					OpenHourPeriod: {
						EndTime: "15:00:00",
						StartTime: "10:00:00"
					}
				}
			],
			AbsenceProbabilityEnabled: true,
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
					Time: "00:00:00",
					TimeLineDisplay: "00:00",
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: "24:00:00",
					TimeLineDisplay: "24:00",
					PositionPercentage: 1,
					TimeFixedFormat: null
				}],
		};
	}

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate) {
		var result = [];
		for (var i = 0; i < (24 * 60) / constants.probabilityIntervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate,
				StartTime: moment(formattedDate)
					.startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * i, 'minutes')
					.format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(formattedDate)
					.startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * (i + 1), 'minutes')
					.format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: constants.probabilityIntervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	test("Calculate absence probability boundaries for dayoff", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		//probability start: 9 * 60 + 30 = 570, end 16 * 60 + 45 = 1005
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1005);
	});

	test("Calculate absence probability boundaries for full day absence", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		//probability start: 9 * 60 + 30 = 570, end 16 * 60 + 45 = 1005
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1005);
	});

	test("Calculate overtime probability boundaries for dayoff", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.overtime, []);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		//probability start: 9 * 60 + 30 = 570, end 16 * 60 + 45 = 1005
		//site open period: 10 * 60 = 600, 15 * 60 = 900
		equal(vm.probabilityStartMinutes, 600);
		equal(vm.probabilityEndMinutes, 900);
	});

	test("Calculate overtime probability boundaries for full day absence", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.overtime, []);

		equal(vm.lengthPercentagePerMinute, 1 / constants.totalMinutesOfOneDay);
		//probability start: 9 * 60 + 30 = 570, end 16 * 60 + 45 = 1005
		//site open period: 10 * 60 = 600, 15 * 60 = 900
		equal(vm.probabilityStartMinutes, 600);
		equal(vm.probabilityEndMinutes, 900);
	});

	test("Calculate absence probability boundaries for normal day", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [
				{
					Time: "09:00:00",
					TimeLineDisplay: "09:00",
					PositionPercentage: 0,
					TimeFixedFormat: null
				},{
					Time: "17:00:00",
					TimeLineDisplay: "17:00",
					PositionPercentage: 1,
					TimeFixedFormat: null
				}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / ((17 - 9) * 60));
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1005);
	});

	// Should show overtime probability in intersection of timeline, open hour period and probility start / end
	test("Calculate overtime probability boundaries for normal day", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [
			{
				Time: "09:00:00",
				TimeLineDisplay: "09:00",
				PositionPercentage: 0,
				TimeFixedFormat: null
			},{
				Time: "17:00:00",
				TimeLineDisplay: "17:00",
				PositionPercentage: 1,
				TimeFixedFormat: null
			}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / ((17 - 9) * 60));
		//probability start: 9 * 60 + 30 = 570, end 16 * 60 + 45 = 1005
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1005);
	});

	test("Calculate absence probability boundaries for cross day schedule ends today", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(basedDate).startOf('day').subtract('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: "00:00:00",
				TimeLineDisplay: "00:00",
				PositionPercentage: 0,
				TimeFixedFormat: null
			},{
				Time: "17:00:00",
				TimeLineDisplay: "17:00",
				PositionPercentage: 1,
				TimeFixedFormat: null
			}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / (17 * 60));
		//(-1) 22:00 ~ 16:45
		equal(vm.probabilityStartMinutes, 0);
		equal(vm.probabilityEndMinutes, 1005);
	});

	test("Calculate absence probability boundaries for cross day schedule that ends tomorrow", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).startOf('day').add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: "09:00:00",
				TimeLineDisplay: "09:00",
				PositionPercentage: 0,
				TimeFixedFormat: null
			},{
				Time: "24:00:00",
				TimeLineDisplay: "24:00",
				PositionPercentage: 1,
				TimeFixedFormat: null
			}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.absence, []);

		equal(vm.lengthPercentagePerMinute, 1 / ((24 - 9) * 60));
		// 09:30 ~ 02:00(+1)
		equal(vm.probabilityStartMinutes, 570);
		equal(vm.probabilityEndMinutes, 1440);
	});

	test("Calculate overtime probability boundaries for cross day schedule end today", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].StartTime = moment(basedDate).startOf('day').subtract('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: "00:00:00",
				TimeLineDisplay: "00:00",
				PositionPercentage: 0,
				TimeFixedFormat: null
			},{
				Time: "17:00:00",
				TimeLineDisplay: "17:00",
				PositionPercentage: 1,
				TimeFixedFormat: null
			}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.overtime, []);


		equal(vm.lengthPercentagePerMinute, 1 / (17 * 60));
		//scheduel (-1)22:00 ~ 16:45
		//site open period: 10 * 60 = 600, 15 * 60 = 900
		equal(vm.probabilityStartMinutes, 600);
		equal(vm.probabilityEndMinutes, 900);
	});

	test("Calculate overtime probability boundaries for cross day schedule will end tomorrow", function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).startOf('day').add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: "09:00:00",
				TimeLineDisplay: "09:00",
				PositionPercentage: 0,
				TimeFixedFormat: null
			},{
				Time: "24:00:00",
				TimeLineDisplay: "24:00",
				PositionPercentage: 1,
				TimeFixedFormat: null
			}];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null, undefined);
		week.initializeData(fakeScheduleData);
		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.DayViewModel(fakeScheduleData.Days[0], week);

		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, week.timeLines(),
			constants.probabilityType.overtime, []);

		equal(vm.lengthPercentagePerMinute, 1 / ((24 -9) * 60));
		//scheduel 09:30 ~ 02:00(+1)
		//site open period: 10 * 60 = 600, 15 * 60 = 900
		equal(vm.probabilityStartMinutes, 600);
		equal(vm.probabilityEndMinutes, 900);
	});

	test("should calculate absence probability boundaries for cross day schedule that ends tomorrow for mobile day view", function () {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = moment(basedDate).add('day', 1).startOf('day').add('hour', 2).format('YYYY-MM-DDTHH:mm:ss');

		var dayViewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		dayViewModel.readData(
			{
				Schedule: fakeScheduleData.Days[0],
				TimeLine: [
					{
						Time: "09:15:00",
						TimeLineDisplay: "09:15",
						PositionPercentage: 0,
						TimeFixedFormat: null
					}, {
						Time: "1.02:15:00",
						TimeLineDisplay: "02:15",
						PositionPercentage: 1,
						TimeFixedFormat: null
					}]
			}
		);
		var probabilityDataDay1 = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(basedDate);
		var probabilityDataDay2 = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(moment(basedDate).add(1, 'days').format(
			'YYYY-MM-DD'
		));

		var schedulePeriodStart = moment(fakeScheduleData.Days[0].Periods[0].StartTime);
		var schedulePeriodEnd = moment(fakeScheduleData.Days[0].Periods[fakeScheduleData.Days[0].Periods.length - 1].EndTime);

		var filteredProbabilityData = probabilityDataDay1.concat(probabilityDataDay2).filter(function (probability) {
			return (
				schedulePeriodStart <= moment(probability.StartTime) &&
				moment(probability.EndTime) <= schedulePeriodEnd
			);
		});

		var boundary = Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, dayViewModel.timeLines(),
			constants.probabilityType.absence, filteredProbabilityData, null, true);

		equal(boundary.lengthPercentagePerMinute, 1 / ((26.25 - 9.25) * 60));
		equal(boundary.probabilityStartMinutes, 9 * 60 + 30);
		equal(boundary.probabilityEndMinutes, 24 * 60 + 2 * 60);
	});
});