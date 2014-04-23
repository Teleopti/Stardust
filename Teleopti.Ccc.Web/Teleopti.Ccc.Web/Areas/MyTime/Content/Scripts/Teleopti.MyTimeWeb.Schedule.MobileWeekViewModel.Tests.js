/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />

$(document).ready(function () {
    module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");

    test("should read scheduled days", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

        vm.readData({
            Days: [{
            }]
        });

        equal(vm.dayViewModels().length, 1);
    });
    
    test("should read date", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

        vm.readData({
            Days: [{
                FixedDate: "2014-04-14"
            }]
        });

        equal(vm.dayViewModels()[0].fixedDate(), "2014-04-14");
    });
    
    test("should load shift category data", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            Days: [{
                Summary: {
                    Title: "Early",
                    TimeSpan: "09:00-18:00",
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
            Days: [{
                IsDayOff: true
            }]
        });

        //equal(vm.dayViewModels()[0].isDayOff(), true);
    });

	test("should read week day header titles", function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		vm.readData({
			Days: [{ Header: { Title: "Monday" } }]
		});
		equal(vm.dayViewModels()[0].weekDayHeaderTitle(), "Monday");
	});
});