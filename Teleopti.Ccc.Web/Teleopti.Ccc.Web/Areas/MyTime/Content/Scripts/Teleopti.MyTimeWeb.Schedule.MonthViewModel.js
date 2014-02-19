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
    this.absenceName = scheduleDate.Absence ? scheduleDate.Absence.Name : '';
    this.absenceShortName = scheduleDate.Absence ? scheduleDate.Absence.ShortName : '';
    this.isDayOff = scheduleDate.IsDayOff;
    this.shiftName = scheduleDate.Shift ? scheduleDate.Shift.Name : '';
    this.shiftShortName = scheduleDate.Shift ? scheduleDate.Shift.ShortName : '';
    this.shiftTimeSpan = scheduleDate.Shift ? scheduleDate.Shift.TimeSpan: '';
    this.shiftWorkingHours = scheduleDate.Shift ? scheduleDate.Shift.WorkingHours : '';
    this.shiftColor = scheduleDate.Shift ? scheduleDate.Shift.Color : '';
    
    this.isOutsideMonth = (selectedDate.month() != currentDate.month());
};

Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel = function () {
    this.dayViewModels = ko.observableArray();
};

Teleopti.MyTimeWeb.Schedule.MonthViewModel = function () {
    var self = this;
    this.weekViewModels = ko.observableArray();
    this.weekDayNames = ko.observableArray();

    this.selectedDate = ko.observable(moment());

    this.formattedSelectedDate = ko.computed(function() {
        return self.selectedDate().format('MMMM YYYY');
    });
    
    this.nextMonth = function() {
        var date = self.selectedDate().clone();
        date.add('months', 1);
        self.selectedDate(date);
    };

    this.previousMonth = function () {
        var date = self.selectedDate().clone();
        date.add('months', -1);
        self.selectedDate(date);
    };

    this.week = function (day) {
        var d = day.currentDate;
        Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
    };

    this.readData = function (data) {
        self.weekDayNames(data.DayHeaders);
        self.selectedDate(moment(data.FixedDate, 'YYYY-MM-DD'));
        var newWeek;
        for (var i = 0; i < data.ScheduleDays.length; i++) {
            if (i % 7 == 0) {
                if (newWeek)
                    self.weekViewModels.push(newWeek);
                newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
            }

            var newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[i], self.selectedDate());
            newWeek.dayViewModels.push(newDay);
        }
        self.weekViewModels.push(newWeek);
    };
};
