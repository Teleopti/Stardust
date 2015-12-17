/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");

	test("should read absence report permission", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
			}],
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

	test("should read date", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
				FixedDate: "2014-04-14"
			}]
		});

		equal(vm.dayViewModels()[0].fixedDate(), "2014-04-14");
	});

	test("should load shift category data", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
				Summary: {
					Title: "Early",
					TimeSpan: "09:00-18:00",
					StyleClassName: "dayoff striped",
					Color: "rgb(0, 0, 0)"
				}
			}]
		});

		equal(vm.dayViewModels()[0].summaryName(), "Early");
		equal(vm.dayViewModels()[0].summaryTimeSpan(), "09:00-18:00");
		equal(vm.dayViewModels()[0].summaryColor(), "rgb(0, 0, 0)");
	});


	test("should read dayoff data", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
				Summary: {
					StyleClassName: "dayoff striped",
				}
			}]
		});

		equal(vm.dayViewModels()[0].isDayoff(), true);
	});

	test("should indicate has shift", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
				Summary: {
					Color: ""
				}
			}]
		});

		equal(vm.dayViewModels()[0].hasShift, true);
	});

	test("should read week day header titles", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{ Header: { Title: "Monday" } }]
		});
		equal(vm.dayViewModels()[0].weekDayHeaderTitle(), "Monday");
	});
});