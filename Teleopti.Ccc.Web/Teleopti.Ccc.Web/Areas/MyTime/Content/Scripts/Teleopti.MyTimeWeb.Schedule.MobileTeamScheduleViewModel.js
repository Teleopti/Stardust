/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel = function (weekStart, parent, dataService) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.selectedDate = ko.observable(moment());
	self.displayDate = ko.observable(self.selectedDate().format('YYYY-MM-DD'));
	self.selectedDate.subscribe(function (value) {
		self.displayDate(value.format('YYYY-MM-DD'));
	});
	self.previousDay = function(){
		var previousDate = moment(self.selectedDate()).add(-1, 'days');
		self.displayDate(previousDate.format('YYYY-MM-DD'));
		self.selectedDate(previousDate); 
	};

	self.nextDay = function(){
		var nextDate = moment(self.selectedDate()).add(1, 'days');
		self.displayDate(nextDate.format('YYYY-MM-DD'));
		self.selectedDate(nextDate);
	};
};