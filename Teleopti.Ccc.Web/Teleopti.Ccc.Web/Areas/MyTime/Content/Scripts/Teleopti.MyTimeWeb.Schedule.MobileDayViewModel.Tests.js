/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileDayViewModel");

	var userTexts = {
		"subjectColon": "Subject:",
		"locationColon": "Location:",
		"descriptionColon": "Description:"
	};

	test("should read date", function () {
		var scheduleDay = {
			FixedDate: "2014-04-14"
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, false, true, userTexts);

		equal(vm.fixedDate(), "2014-04-14");
		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should read permission", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel({}, false, true, userTexts);

		equal(vm.absenceReportPermission(), false);
		equal(vm.overtimeAvailabilityPermission(), true);
	});

	test("should load shift category data", function () {
		var scheduleDay = {
			Summary: {
				Title: "Early",
				TimeSpan: "09:00-18:00",
				StyleClassName: "dayoff striped",
				Color: "rgb(0, 0, 0)"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, true, true, userTexts);
		equal(vm.summaryName(), scheduleDay.Summary.Title);
		equal(vm.summaryTimeSpan(), scheduleDay.Summary.TimeSpan);
		equal(vm.summaryColor(), scheduleDay.Summary.Color);
		equal(vm.summaryStyleClassName(), scheduleDay.Summary.StyleClassName);
		equal(vm.backgroundColor, scheduleDay.Summary.Color);
	});

	test("should read dayoff data", function () {
		var scheduleDay = {
			Summary: {
				StyleClassName: "dayoff striped"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, true, true, userTexts);
		equal(vm.isDayoff(), true);
	});

	test("should indicate has shift", function () {
		var scheduleDay = {
			Summary: {
				Color: ""
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, true, true, userTexts);
		equal(vm.hasShift, true);
	});

	test("should read week day header titles", function () {
		var scheduleDay = {
			Header: {
				Title: "Monday"
			}
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, true, true, userTexts);
		equal(vm.weekDayHeaderTitle(), "Monday");
	});

	test("should read summary timespan when there is overtime and overtime availability", function () {
		var scheduleDay = {
			Summary: {
				Color: null,
				TimeSpan: null
			},
			Periods: [
				{
					IsOvertime: true,
					TimeSpan: "9:00 AM - 11:30 AM",
					Title: "Phone"
				},
				{
					IsOvertime: true,
					TimeSpan: "11:30 AM - 11:45 AM",
					Title: "Short Break"
				},
				{
					IsOvertime: true,
					TimeSpan: "11:45 AM - 2:00 PM",
					Title: "Phone"
				},
				{
					IsOvertimeAvailability: true,
					TimeSpan: "7:00 AM - 2:00 PM",
					Title: "Overtime Availability"
				}
			],
			HasOvertime: true
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay, true, true, userTexts);
		equal(vm.summaryTimeSpan(), "9:00 AM -  2:00 PM");
		equal(vm.layers.length, 4);
	});
});