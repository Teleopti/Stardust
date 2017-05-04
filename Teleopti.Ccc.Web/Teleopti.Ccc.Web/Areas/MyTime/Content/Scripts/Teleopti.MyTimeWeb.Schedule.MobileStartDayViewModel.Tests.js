/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function() {
    module("Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel");

    test("should set current date", function () {
        var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

        viewModel.setCurrentDate(moment().format('YYYY-MM-DD'));

        equal(viewModel.selectedDate(), moment().format('YYYY-MM-DD'));
    });

    test("should set display date", function () {
        var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

        var rawData = {
            DisplayDate: moment().format('YYYY-MM-DD'),
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

        equal(viewModel.displayDate(), rawData.DisplayDate);
    });

    test("should set summary color", function () {
        var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

        var rawData = {
            DisplayDate: null,
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
            DisplayDate: null,
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
            DisplayDate: null,
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
            DisplayDate: null,
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
});