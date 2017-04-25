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
});