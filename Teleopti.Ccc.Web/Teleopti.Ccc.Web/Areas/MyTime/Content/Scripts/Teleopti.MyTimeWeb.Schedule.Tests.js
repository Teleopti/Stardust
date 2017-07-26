/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/hasher/hasher.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule");

	var hash = "";
	var fakeAddRequestViewModel = Teleopti.MyTimeWeb.Schedule.FakeData.fakeAddRequestViewModel;
	var momentWithLocale = function (date) { return moment(date).locale('en-gb'); };
	var basedDate = momentWithLocale(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD');
	var userTexts = Teleopti.MyTimeWeb.Schedule.FakeData.userTexts;

	function getFakeScheduleData() {
		return Teleopti.MyTimeWeb.Schedule.FakeData.getFakeScheduleData();
	}

	function initUserTexts() {
		Teleopti.MyTimeWeb.Common.SetUserTexts(userTexts);
	}

	function setupHash() {
		this.hasher = {
			initialized: {
				add: function () { }
			},
			changed: {
				add: function () { }
			},
			init: function () { },
			setHash: function (data) { hash = "#" + data; }
		};
	}

	test("should read absence report permission", function () {
		initUserTexts();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.absenceReportPermission(), true);
	});

	test("should read scheduled days", function () {
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
		equal(vm.days()[0].noteMessage().indexOf(fakeScheduleData.Days[0].Note.Message) > -1, true);
		equal(vm.days()[0].seatBookings(), fakeScheduleData.Days[0].SeatBookings);
	});

	test("should read timelines", function () {
		initUserTexts();
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

	test("should refresh and modify url after changing date", function () {
		setupHash();
		initUserTexts();
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);
		vm.nextWeek();
		equal(hash.indexOf(moment(basedDate).add('days', 7).format('YYYY/MM/DD')) > 0, true);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
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

		equal("#Schedule/MobileDay/", hash);
	});

	test("should get correct minutes when there is full day time line", function () {
		var timeline = { Time: "1.00:00:00", TimeLineDisplay: "00:00", PositionPercentage: 1, TimeFixedFormat: null };
		var timelineViewModel = new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(timeline, 100, 0);
		equal(timelineViewModel.minutes, 1440);
	});

	test("should read overtime request toggle", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_OvertimeRequest_44558") return false;
			return false;
		};
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.RequestPermission.OvertimeRequestPermission = true;

		vm.initializeData(fakeScheduleData);

		equal(vm.isOvertimeRequestAvailable(), false);
	});

	test("should read overtime request permission", function () {
		initUserTexts();
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_OvertimeRequest_44558") return true;
			return false;
		};
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.isOvertimeRequestAvailable(), true);
	});
});
