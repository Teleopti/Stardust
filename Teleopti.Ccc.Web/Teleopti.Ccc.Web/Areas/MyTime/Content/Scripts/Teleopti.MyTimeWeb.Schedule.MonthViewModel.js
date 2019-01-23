Teleopti.MyTimeWeb.Schedule.MonthViewModel = function() {
	var self = this;

	self.weekViewModels = ko.observableArray();
	self.weekDayNames = ko.observableArray();

	self.selectedDate = ko.observable(
		Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
			? moment(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash)
			: moment()
	);

	self.formattedSelectedDate = ko.computed(function() {
		return Teleopti.MyTimeWeb.Common.FormatMonth(self.selectedDate());
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

	self.today = function() {
		var probabilityPart = getProbabilityUrlPart();
		Teleopti.MyTimeWeb.Portal.NavigateTo('Schedule/Week' + probabilityPart);
	};

	self.week = function(day) {
		var probabilityPart = getProbabilityUrlPart();
		var d = day.currentDate;
		if (typeof d === 'undefined') {
			Teleopti.MyTimeWeb.Portal.NavigateTo('Schedule/Week' + probabilityPart);
		} else {
			Teleopti.MyTimeWeb.Portal.NavigateTo(
				'Schedule/Week' +
					Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')) +
					probabilityPart
			);
		}
	};

	function getProbabilityUrlPart() {
		var probabilityPart = '';
		var probability = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
		if (probability) {
			probabilityPart = '/Probability/' + probability;
		}
		return probabilityPart;
	}

	self.month = function() {
		var d = self.selectedDate();
		Teleopti.MyTimeWeb.Portal.NavigateTo(
			'Schedule/Month' + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD'))
		);
	};

	self.readData = function(data) {
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
				if (newWeek) self.weekViewModels.push(newWeek);
				newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
			}
			var newDay;
			if (useJalaaliCalendar) {
				newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(
					data.ScheduleDays[base - count],
					self.selectedDate()
				);
			} else {
				newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[i], self.selectedDate());
			}

			count++;
			newWeek.dayViewModels.push(newDay);
		}
		self.weekViewModels.push(newWeek);
	};
};