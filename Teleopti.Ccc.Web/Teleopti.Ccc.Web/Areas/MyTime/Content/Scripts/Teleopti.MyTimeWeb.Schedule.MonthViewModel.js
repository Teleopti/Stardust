/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
    Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Schedule) === 'undefined') {
    Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MonthDayViewModel = function (date, selectedDate) {
    
    this.date = date.format('YYYY-MM-DD');
    this.dayOfMonth = date.date();

    this.isOutsideMonth = (selectedDate.month() != date.month());
};

Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel = function () {
    this.dayViewModels = ko.observableArray();
};

Teleopti.MyTimeWeb.Schedule.MonthViewModel = function () {
    this.weekViewModels = ko.observableArray();
};


