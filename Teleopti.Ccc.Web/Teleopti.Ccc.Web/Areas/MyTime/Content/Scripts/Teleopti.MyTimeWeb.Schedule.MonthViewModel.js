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

Teleopti.MyTimeWeb.Schedule.MonthDayViewModel = function (scheduleDate, selectedDate) {
    var currentDate = moment(scheduleDate.FixedDate, 'YYYY-MM-DD');

    this.currentDate = currentDate;
    this.date = scheduleDate.FixedDate;
    this.dayOfMonth = currentDate.date();
    this.isWorkingDay = scheduleDate.IsWorkingDay;
    this.isNotWorkingDay = scheduleDate.IsNotWorkingDay;
    this.displayColor = scheduleDate.DisplayColor;
    
    this.isOutsideMonth = (selectedDate.month() != currentDate.month());
};

Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel = function () {
    this.dayViewModels = ko.observableArray();
};

Teleopti.MyTimeWeb.Schedule.MonthViewModel = function (monthData, selectedDate) {
    this.weekViewModels = ko.observableArray();
    this.weekDayNames = monthData.DayHeaders;
    this.selectedDate = selectedDate;
    this.formattedSelectedDate = selectedDate.format('MMMM YYYY');

    this.nextMonth = function() {
        var date = selectedDate.clone();
        date.add('months', 1);
        date.startOf('month');
        Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
    };

    this.previousMonth = function () {
        var date = selectedDate.clone();
        date.add('months', -1);
        date.startOf('month');
        Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
    };
    this.week = function (day) {
        var d = day.currentDate;
        Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
    };
};


