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
	self.desktop = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week");
	};

	self.readData = function(data) {

		ko.utils.arrayForEach(data.Days, function(scheduleDay) {

			var vm = new Teleopti.MyTimeWeb.Schedule.MobileDayViewModel();
			vm.readData(scheduleDay);

			self.dayViewModels.push(vm);
		});
	};
};
Teleopti.MyTimeWeb.Schedule.MobileDayViewModel = function () {
        var self = this;
        self.shiftName = ko.observable();
        self.shiftTimeSpan = ko.observable();
        self.shiftColor = ko.observable();
        self.absenceName = ko.observable();
        self.absenceIsFullDayAbsence = ko.observable();
        self.isDayOff = ko.observable();
        self.hasAbsence = ko.observable();
        self.hasShift = ko.observable();
        self.fixedDate = ko.observable();
        self.readData = function (data) {
            self.shiftName(data.Shift ? data.Shift.Name : null);
            self.shiftTimeSpan(data.Shift ? data.Shift.TimeSpan : null);
            self.shiftColor(data.Shift ? data.Shift.Color : null);
            self.absenceName(data.Absence ? data.Absence.Name : null);
            self.absenceIsFullDayAbsence(data.Absence ? data.Absence.IsFullDayAbsence : null);
            self.isDayOff(data.IsDayOff);
            self.hasAbsence(data.Absence ? true : false);
            self.hasShift(data.Shift ? true : false);
            self.fixedDate(data.FixedDate);
        };
 };

