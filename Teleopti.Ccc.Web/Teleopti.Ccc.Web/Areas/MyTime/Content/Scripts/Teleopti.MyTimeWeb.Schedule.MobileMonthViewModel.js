Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel = function(parent) {
	var self = this;

	self.isLoading = ko.observable(false);
	self.selectedDateSubscription = null;
	self.hasAbsenceOrOvertime = ko.observable(false);
	self.hasAbsenceAndOvertime = ko.observable(false);

	self.weekViewModels = ko.observableArray();
	self.weekDayNames = ko.observableArray();

	self.unreadMessageCount = ko.observable();
	self.asmEnabled = ko.observable(false);
	self.selectedDate = ko.observable(
		Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
			? moment(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash)
			: moment()
	);
	self.getSelectedDate = ko.computed(self.selectedDate).extend({ throttle: 50 });

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

	self.isWithinSelected = function(startDate, endDate) {
		var weekViewModels = self.weekViewModels();
		var periodStartDateMoment = moment(weekViewModels[0].dayViewModels()[0].date);
		var periodEndDateMoment = moment(weekViewModels[weekViewModels.length - 1].dayViewModels()[6].date);
		return periodStartDateMoment <= moment(startDate) && moment(endDate) <= periodEndDateMoment;
	};

	self.readData = function(data) {
		self.weekDayNames(data.DayHeaders);
		self.weekViewModels([]);
		self.unreadMessageCount(data.UnReadMessageCount);
		self.asmEnabled(data.AsmEnabled);
		var newWeek;
		for (var i = 0; i < data.ScheduleDays.length; i++) {
			if (i % 7 === 0) {
				if (newWeek) self.weekViewModels.push(newWeek);
				newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
			}
			var newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[i], self.selectedDate());
			newWeek.dayViewModels.push(newDay);
		}
		self.weekViewModels.push(newWeek);
		setUseFullHeightForDateCells();
		setSelectedDateSubscription();
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

	function setSelectedDateSubscription() {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();

		self.selectedDateSubscription = self.getSelectedDate.subscribe(function(date) {
			self.isLoading(true);
			parent.ReloadSchedule(date);
		});
	}
};
