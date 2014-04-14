/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />

$(document).ready(function () {
    module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");

    test("should read scheduled days", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

        vm.readData({
            ScheduleDays: [{
            }]
        });

        equal(vm.dayViewModels().length, 1);
    });
    
    test("should load shift category data", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            ScheduleDays: [{
                Shift: {
                    Name: "Early",
                    TimeSpan: "09:00-18:00",
                    Color: "green"
                }
            }]
        });

        equal(vm.dayViewModels()[0].shiftName(), "Early");
        equal(vm.dayViewModels()[0].shiftTimeSpan(), "09:00-18:00");
        equal(vm.dayViewModels()[0].shiftColor(), "green");
    });
    
    test("should read absence data", function () {

        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

        vm.readData({
            ScheduleDays: [{
                Absence: {
                    Name: "Illness",
                    IsFullDayAbsence: true
                }
            }]
        });

        equal(vm.dayViewModels()[0].absenceName(), "Illness");
        equal(vm.dayViewModels()[0].absenceIsFullDayAbsence(), true);
    });

    test("should read dayoff data", function () {

        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

        vm.readData({
            ScheduleDays: [{
                IsDayOff: true
            }]
        });

        equal(vm.dayViewModels()[0].isDayOff(), true);
    });

    test("should not indicate absence when no data", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            ScheduleDays: [{}]
        });

        equal(vm.dayViewModels()[0].hasAbsence(), false);
    });
    
    test("should indicate absence when the scheduleday has absence", function () {
      
        vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            ScheduleDays: [{ Absence: { Name: "Illness" } }]
        });
        equal(vm.dayViewModels()[0].hasAbsence(), true);
    });

    test("should not indicate shift when no shift data available", function () {
        var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            ScheduleDays: [{}]
        });
        equal(vm.dayViewModels()[0].hasShift(), false);
    });
    
    test("should indicate shift when there is shift data", function () {
        vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
        vm.readData({
            ScheduleDays: [{ Shift: { Name: "Late" } }]
        });
        equal(vm.dayViewModels()[0].hasShift(), true);
    });
});