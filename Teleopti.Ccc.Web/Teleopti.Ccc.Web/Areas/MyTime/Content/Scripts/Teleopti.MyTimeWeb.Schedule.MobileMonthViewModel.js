/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />

if (typeof Teleopti === "undefined") {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === "undefined") {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === "undefined") {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel = function() {
	var self = this;

	self.unreadMessageCount = 0;
	self.selectedDate = ko.observable(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ? moment(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash) : moment());

	self.formattedSelectedDate = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatMonthShort(self.selectedDate());
	});

	self.nextMonth = function() {
		var date = self.selectedDate().clone();
		date.add('months', 1);
		self.selectedDate(date);
	};

	self.previousMonth = function() {
		var date = self.selectedDate().clone();
		date.add('months', -1);
		self.selectedDate(date);
	};
};