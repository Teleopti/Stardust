/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function() {
	module("Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel");

	test("should get current date", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		equal(viewModel.selectedDate().format('YYYY-MM-DD'), moment().format('YYYY-MM-DD'));
	});

	test("should set display date", function () {
		 Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: "DD/MM/YYYY",
			UseJalaaliCalendar: false
		});
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.displayDate(), moment().format('DD/MM/YYYY'));
	});

	test("should display date correctly when in Persian date format", function () {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: "DD/MM/YYYY",
			UseJalaaliCalendar: true
		});
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.displayDate(), moment(rawData.Date).format(Teleopti.MyTimeWeb.Common.DateFormat));
	});

	test("should set summary color", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: "0, 255, 0",
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryColor(), rawData.Schedule.Summary.Color);
	});

	test("should set summary name", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: "Early",
					TimeSpan: null
				},
				Header:{Title: null}
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryName(), rawData.Schedule.Summary.Title);
	});

	test("should set summary time", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: "9:00-18:00"
				},
				Header:{Title: null}
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryTime(), rawData.Schedule.Summary.TimeSpan);
	});

	test("should set whether current day is day off", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				IsDayOff: true
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.isDayOff(), rawData.Schedule.IsDayOff);
	}); 

	test("should set to next date", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var today = moment().format('YYYY-MM-DD');
		viewModel.selectedDate(today);

		viewModel.nextDay();

		equal(viewModel.selectedDate().format("YYYY-MM-DD"), moment(today).add(1, 'days').format("YYYY-MM-DD"));
	});

	test("should set to previous date", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var today = moment().format('YYYY-MM-DD');
		viewModel.selectedDate(today);

		viewModel.previousDay();

		equal(viewModel.selectedDate().format("YYYY-MM-DD"), moment(today).add(-1, 'days').format("YYYY-MM-DD"));
	});

	test("should call out menu list when clicking plus icon at bottom right", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.enableMenu();

		equal(viewModel.menuIsVisible(), true);
	});

	test("should hide menu list when clicking on menu item or outside", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.enableMenu();

		viewModel.disableMenu();

		equal(viewModel.menuIsVisible(), false);
	});

	test("should hide plus icon after calling out menu list", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.enableMenu();

		equal(viewModel.menuIconIsVisible(), false);
	});

	test("should not show overtime availability command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				OvertimeAvailabilityPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.overtimeAvailabilityPermission(), false);
	});

	test("should not show absence reporting command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				AbsenceReportPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.absenceReportPermission(), false);
	});

	test("should not show absence reporting command item if selected date is neigther today nor tomorrow", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				AbsenceReportPermission: true
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.absenceReportPermission(), true);
		equal(viewModel.showAbsenceReportingCommandItem(), true);

		rawData.Date = moment().add('day', 3).format('YYYY-MM-DD');
		viewModel.readData(rawData);

		equal(viewModel.absenceReportPermission(), true);
		equal(viewModel.showAbsenceReportingCommandItem(), false);
	});

	test("should not show text request command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				TextRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.textRequestPermission(), false);
	});

	test("should not show absence request command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				AbsenceRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.absenceRequestPermission(), false);
	});

	test("should not show shift trade request command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				ShiftTradeRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.shiftTradeRequestPermission(), false);
	});

	test("should not show post shift for trade command item without permission", function () {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header:{Title: null}
			},
			RequestPermission:{
				ShiftExchangePermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.shiftExchangePermission(), false);
	});
});