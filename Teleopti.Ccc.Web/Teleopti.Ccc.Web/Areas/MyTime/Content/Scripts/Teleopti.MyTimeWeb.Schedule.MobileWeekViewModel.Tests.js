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


	test("should read summary timespan when there is overtime and overtime availability", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [
				{
					Display: null
				}
			],
			Days: [
				{
					Summary: {
						Color: null,
						TimeSpan:null
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
				}
			]
		});

		equal(vm.dayViewModels()[0].summaryTimeSpan(), "9:00 AM -  2:00 PM");
	});
});