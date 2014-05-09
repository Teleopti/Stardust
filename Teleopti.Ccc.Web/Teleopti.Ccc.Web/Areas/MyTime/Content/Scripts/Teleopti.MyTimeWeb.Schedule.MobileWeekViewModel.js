/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
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

Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel = function() {
	var self = this;

	self.dayViewModels = ko.observableArray();
	self.displayDate = ko.observable();
	self.nextWeekDate = ko.observable(moment());
	self.previousWeekDate = ko.observable(moment());
	self.selectedDate = ko.observable(moment().startOf('day'));
	self.selectedDateSubscription = null;

	self.setCurrentDate = function (date) {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();
		self.selectedDate(date);
		self.selectedDateSubscription = self.selectedDate.subscribe(function (d) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
		});
	};

	self.desktop = function() {
		var date = self.selectedDate();
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
	}

	self.nextWeek = function () {
		self.selectedDate(self.nextWeekDate());
	};

	self.previousWeek = function () {
		self.selectedDate(self.previousWeekDate());
	};

	self.readData = function(data) {

		ko.utils.arrayForEach(data.Days, function(scheduleDay) {

			var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay);

			self.dayViewModels.push(vm);
		});
		self.displayDate(data.PeriodSelection.Display);
	};
};

Teleopti.MyTimeWeb.Schedule.MobileDayViewModel = function(scheduleDay) {
	var self = this;
	self.summaryName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Title : null);
	self.summaryTimeSpan = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.TimeSpan : null);
	self.summaryColor = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.Color : null);
	self.fixedDate = ko.observable(scheduleDay.FixedDate);
	self.weekDayHeaderTitle = ko.observable(scheduleDay.Header ? scheduleDay.Header.Title : null);
	self.summaryStyleClassName = ko.observable(scheduleDay.Summary ? scheduleDay.Summary.StyleClassName : null);
	self.isDayoff = ko.computed(function() {
		if (self.summaryStyleClassName() != undefined && self.summaryStyleClassName() != null) {
			return self.summaryStyleClassName() == "dayoff striped";
		}
		return false;
	});
	self.hasShift = ko.computed(function() {
		if (scheduleDay.Periods != null && scheduleDay.Periods.length > 0)
			return true;
		return false;
	});
	self.hasFulldayAbsence = ko.computed(function() {
		if (self.summaryTimeSpan != undefined && scheduleDay.Periods != null) {
			return true;
		}
		return false;
	});

    self.backgroundColor = scheduleDay.Summary ? scheduleDay.Summary.Color : null;
    self.summaryTextColor = ko.observable(self.backgroundColor ? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor) : 'black');

 };

