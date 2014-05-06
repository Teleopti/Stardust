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

	self.readData = function(data) {

		ko.utils.arrayForEach(data.Days, function(scheduleDay) {

			var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel(scheduleDay);

			self.dayViewModels.push(vm);
		});

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

