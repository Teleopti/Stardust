﻿$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Schedule', {
		setup: function() {
			Teleopti.MyTimeWeb.Common.IsToggleEnabled = function() {};
		},
		teardown: function() {}
	});

	var hash = '';
	var fakeAddRequestViewModel = Teleopti.MyTimeWeb.Schedule.FakeData.fakeAddRequestViewModel;
	var momentWithLocale = function(date) {
		return moment(date).locale('en-gb');
	};
	var basedDate = momentWithLocale(
		Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)
	).format('YYYY-MM-DD');
	var userTexts = Teleopti.MyTimeWeb.Schedule.FakeData.userTexts;

	var fakeLicenseAvailabilityData = false;
	var tempAjax;

	function getFakeScheduleData() {
		return Teleopti.MyTimeWeb.Schedule.FakeData.getFakeScheduleData();
	}

	function initUserTexts() {
		Teleopti.MyTimeWeb.Common.SetUserTexts(userTexts);
	}

	function setupHash() {
		this.hasher = {
			initialized: {
				add: function() {}
			},
			changed: {
				add: function() {}
			},
			init: function() {},
			setHash: function(data) {
				hash = '#' + data;
			}
		};
	}

	test('should not set probability info for month view', function() {
		setupHash();
		var old = Teleopti.MyTimeWeb.Portal.ParseHash;
		Teleopti.MyTimeWeb.Portal.ParseHash = function() {
			return {
				dateHash: '20180209'
			};
		};
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.selectedProbabilityType = 2;

		vm.month();

		equal(hash, '#Schedule/Month/20180209');
		Teleopti.MyTimeWeb.Portal.ParseHash = old;
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should read absence report permission', function() {
		initUserTexts();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.absenceReportPermission(), true);
	});

	test('should read scheduled days', function() {
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);
		equal(vm.days().length, 2);
		equal(vm.days()[0].headerTitle(), fakeScheduleData.Days[0].Header.Title);
		equal(vm.days()[0].summary(), fakeScheduleData.Days[0].Summary.Summary);
		equal(vm.days()[0].summaryTitle(), fakeScheduleData.Days[0].Summary.Title);
		equal(vm.days()[0].summaryTimeSpan(), fakeScheduleData.Days[0].Summary.TimeSpan);
		equal(vm.days()[0].summaryStyleClassName(), fakeScheduleData.Days[0].Summary.StyleClassName);
		equal(vm.days()[0].hasShift, true);
		equal(vm.days()[0].noteMessage.indexOf(fakeScheduleData.Days[0].Note.Message) > -1, true);
		equal(vm.days()[0].seatBookings, fakeScheduleData.Days[0].SeatBookings);
	});

	test('should read timelines', function() {
		initUserTexts();
		var fakeScheduleData = getFakeScheduleData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeScheduleData.TimeLine = [
			{
				Time: '09:15:00',
				TimeLineDisplay: '09:15',
				TimeFixedFormat: null
			}
		];
		for (var i = 10; i <= 17; i++) {
			fakeScheduleData.TimeLine.push({
				Time: i + ':00:00',
				TimeLineDisplay: i + ':00',
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

	test('should refresh and modify url after changing date', function() {
		setupHash();
		initUserTexts();
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);
		vm.nextWeek();
		equal(
			hash.indexOf(
				moment(basedDate)
					.add('days', 7)
					.format('YYYY/MM/DD')
			) > 0,
			true
		);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should get correct minutes when there is full day time line', function() {
		var timeline = { Time: '1.00:00:00', TimeLineDisplay: '00:00', PositionPercentage: 1, TimeFixedFormat: null };
		var timelineViewModel = new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(timeline, 100, 0, true);
		equal(timelineViewModel.minutes, 1440);
	});

	test('should read overtime request permission', function() {
		initUserTexts();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.isOvertimeRequestAvailable(), true);
	});

	test('should increase request count after creating an overtime request', function() {
		var tempTogglFn = Teleopti.MyTimeWeb.Common.IsToggleEnabled;
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function() {
			return true;
		};
		var tempFn1 = Date.prototype.getTeleoptiTime;
		Date.prototype.getTeleoptiTime = function() {
			return new Date('2018-03-05T02:30:00Z').getTime();
		};

		var tempFn2 = Date.prototype.getTeleoptiTimeInUserTimezone;
		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return '2018-03-04';
		};

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		var requestDate = moment().format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);
		var responseData = {};
		responseData.DateFromYear = moment().year();
		responseData.DateFromMonth = moment().month() + 1;
		responseData.DateFromDayOfMonth = moment().date();
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'OvertimeRequests/Save') {
					options.success(responseData);
				}
			}
		};
		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, ajax);

		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		vm.initializeData(getFakeScheduleData());

		var scheduleDayViewModel = $.grep(vm.days(), function(item) {
			return item.fixedDate() === requestDate;
		})[0];
		scheduleDayViewModel.requestsCount(0);

		vm.showAddOvertimeRequestForm();
		var overtimeRequestViewModel = vm.requestViewModel().model;
		overtimeRequestViewModel.Subject('test');
		overtimeRequestViewModel.Subject('overtime request');
		overtimeRequestViewModel.Message('I want to work overtime');
		overtimeRequestViewModel.DateFrom(requestDate);
		overtimeRequestViewModel.StartTime('19:00');
		overtimeRequestViewModel.RequestDuration('03:00');
		overtimeRequestViewModel.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');
		overtimeRequestViewModel.AddRequest();

		equal(scheduleDayViewModel.requestsCount(), 1);

		Date.prototype.getTeleoptiTime = tempFn1;
		Date.prototype.getTeleoptiTimeInUserTimezone = tempFn2;

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = tempTogglFn;
	});
});
