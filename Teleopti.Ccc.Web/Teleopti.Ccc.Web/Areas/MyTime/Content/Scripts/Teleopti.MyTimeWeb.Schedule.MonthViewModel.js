﻿/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />

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
	

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		this.dayOfMonth = currentDate.jDate();
		this.isOutsideMonth = (selectedDate.jMonth() != currentDate.jMonth());
	} else
	{
		this.dayOfMonth = currentDate.date();
		this.isOutsideMonth = (selectedDate.month() != currentDate.month());
	}
	
    this.absenceName = scheduleDate.Absence ? scheduleDate.Absence.Name : null;
    this.absenceShortName = scheduleDate.Absence ? scheduleDate.Absence.ShortName : null;
    this.hasAbsence = this.absenceName != null;
    this.isFullDayAbsence = scheduleDate.Absence ? scheduleDate.Absence.IsFullDayAbsence : null;
    this.hasOvertime = scheduleDate.HasOvertime;

    this.isDayOff = scheduleDate.IsDayOff;
	
    this.shiftName = scheduleDate.Shift ? scheduleDate.Shift.Name : null;
    this.shiftShortName = scheduleDate.Shift ? scheduleDate.Shift.ShortName : null;
	this.shiftTimeSpan = scheduleDate.Shift ? scheduleDate.Shift.TimeSpan : null;
    this.shiftWorkingHours = scheduleDate.Shift ? scheduleDate.Shift.WorkingHours : null;
    this.shiftColor = scheduleDate.Shift ? scheduleDate.Shift.Color : null;
    this.hasShift = this.shiftName != null;
    this.backgroundColor = scheduleDate.Shift ? scheduleDate.Shift.Color : null;
    this.shiftTextColor = this.backgroundColor ? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(this.backgroundColor) : 'black';
    
    this.isOutsideMonth = (selectedDate.month() != currentDate.month());
};

Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel = function () {
    this.dayViewModels = ko.observableArray();
};

Teleopti.MyTimeWeb.Schedule.MonthViewModel = function () {
    var self = this;
    this.weekViewModels = ko.observableArray();
    this.weekDayNames = ko.observableArray();

    this.selectedDate = ko.observable(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ? moment(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash) : moment());

    this.formattedSelectedDate = ko.computed(function () {
    	return Teleopti.MyTimeWeb.Common.FormatMonth(self.selectedDate());
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

    self.today = function () {
    	Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week");
    };
	
    this.week = function (day) {
    	var d = day.currentDate;
    	if (typeof(d) === 'undefined') {
    		d = self.selectedDate();
    		d.startOf('month');
    	}
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
    };

	self.month = function () {
    	var d = self.selectedDate();
    	Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
    };
	
	this.readData = function (data) {

		var useJalaaliCalendar = Teleopti.MyTimeWeb.Common.UseJalaaliCalendar;

		if (useJalaaliCalendar) {
			self.weekDayNames(data.DayHeaders.reverse());
		} else {
			self.weekDayNames(data.DayHeaders);
		}
		
        self.selectedDate(moment(data.FixedDate, 'YYYY-MM-DD'));
        var newWeek;
        var count = 1;
	    var base = 0;
        for (var i = 0; i < data.ScheduleDays.length; i++) {
	        
        	if (i % 7 == 0) {
        		base = base + 7;
		        count = 1;
                if (newWeek)
                    self.weekViewModels.push(newWeek);
                newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
        	}
	        var newDay;
        	if (useJalaaliCalendar) {
		        newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[base - count], self.selectedDate());
	        } else {
	        	newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[i], self.selectedDate());
	        }

	        count ++;
            newWeek.dayViewModels.push(newDay);
        }
        self.weekViewModels.push(newWeek);
    };
};
