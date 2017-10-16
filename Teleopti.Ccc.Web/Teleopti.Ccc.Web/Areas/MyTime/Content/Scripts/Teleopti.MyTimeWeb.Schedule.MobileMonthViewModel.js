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

Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel = function(parent) {
	var self = this;

	self.isLoading = ko.observable(false);
	self.selectedDateSubscription = null;
	self.hasAbsenceOrOvertime = ko.observable(false);
	self.hasAbsenceAndOvertime = ko.observable(false);

	self.weekViewModels = ko.observableArray();
	self.weekDayNames = ko.observableArray();

	self.unreadMessageCount = ko.observable();
	self.selectedDate = ko.observable(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ? moment(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash) : moment());

	self.formattedSelectedDate = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatMonthShort(self.selectedDate());
	});

	self.nextMonth = function () {
		removeSelectedDateSubscription();
		var date = self.selectedDate().clone();
		date.add('months', 1);
		self.selectedDate(date);
		parent.ReloadSchedule(date);
	};

	self.previousMonth = function () {
		removeSelectedDateSubscription();
		var date = self.selectedDate().clone();
		date.add('months', -1);
		self.selectedDate(date);
		parent.ReloadSchedule(date);
	};

	self.readData = function(data) {
		self.weekDayNames(data.DayHeaders);
		self.weekViewModels([]);
		self.unreadMessageCount(data.UnReadMessageCount);

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
		setUseFullHeightForDateCells();
		setSelectedDateSubscription(data.FixedDate);
		self.isLoading(false);
	};

	function setUseFullHeightForDateCells() {
		self.hasAbsenceOrOvertime(false);
		self.hasAbsenceAndOvertime(false);

		self.weekViewModels().forEach(function(w) {
			w.dayViewModels().forEach(function(d) {
				if (d && d.hasShift && (d.hasOvertime || d.hasAbsence) && !d.isFullDayAbsence)
					self.hasAbsenceOrOvertime(true);

				if (d && d.hasShift && d.hasOvertime && d.hasAbsence && !d.isFullDayAbsence)
					self.hasAbsenceAndOvertime(true);
			});
		});
	}

	function setSelectedDateSubscription(date) {
		removeSelectedDateSubscription();

		self.selectedDate(moment(date));
		self.selectedDateSubscription = self.selectedDate.subscribe(function(date) {
			parent.ReloadSchedule(date);
		});
	};

	function removeSelectedDateSubscription() {
		if (self.selectedDateSubscription)
			self.selectedDateSubscription.dispose();
	}
};