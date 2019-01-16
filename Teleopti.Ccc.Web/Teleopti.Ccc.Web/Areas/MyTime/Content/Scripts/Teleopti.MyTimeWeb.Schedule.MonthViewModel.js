Teleopti.MyTimeWeb.Schedule.MonthDayViewModel = function(scheduleDate, selectedDate) {
	var self = this;

	var currentDate = moment(scheduleDate.FixedDate, 'YYYY-MM-DD');
	self.currentDate = currentDate;
	self.date = scheduleDate.FixedDate;

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.dayOfMonth = currentDate.jDate();
		self.isOutsideMonth = selectedDate.jMonth() != currentDate.jMonth();
	} else {
		self.dayOfMonth = currentDate.date();
		self.isOutsideMonth = selectedDate.month() != currentDate.month();
	}

	self.seatBookingMessage = getSeatBookingMessage(self.seatBookings);

	self.seatName = function(seatBooking) {
		return seatBooking.LocationPrefix || '' + seatBooking.SeatName + seatBooking.LocationSuffix || '';
	};

	self.navigateToDayView = function() {
		Teleopti.MyTimeWeb.Portal.NavigateTo(
			'Schedule/MobileDay' + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(self.currentDate.format('YYYY-MM-DD'))
		);
	};

	self.absenceName = scheduleDate.Absences ? scheduleDate.Absences[0].Name : null;
	self.absenceShortName = scheduleDate.Absences ? scheduleDate.Absences[0].ShortName : null;
	self.absenceColor = scheduleDate.Absences ? scheduleDate.Absences[0].Color : '';
	self.absenceTextColor = self.absenceColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.absenceColor)
		: 'black';
	self.hasAbsence = self.absenceName != null;
	self.hasMultipleAbsences = scheduleDate.Absences && scheduleDate.Absences.length > 1;
	self.isFullDayAbsence = scheduleDate.Absences ? scheduleDate.Absences[0].IsFullDayAbsence : null;
	self.hasOvertime = scheduleDate.Overtimes != null;
	self.hasMultipleOvertimes = scheduleDate.Overtimes && scheduleDate.Overtimes.length > 1;
	self.overtimeColor = scheduleDate.Overtimes ? scheduleDate.Overtimes[0].Color : '';
	self.overtimeTextColor = self.overtimeColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.overtimeColor)
		: 'black';
	self.hasSeatBooking = scheduleDate.SeatBookings && scheduleDate.SeatBookings.length > 0;
	self.seatBookings = scheduleDate.SeatBookings;
	self.isDayOff = scheduleDate.IsDayOff;
	self.shiftName = scheduleDate.Shift ? scheduleDate.Shift.Name : null;
	self.shiftShortName = scheduleDate.Shift ? scheduleDate.Shift.ShortName : null;
	if (scheduleDate.Shift && scheduleDate.Shift.TimeSpan) {
		var tempTimespan = scheduleDate.Shift.TimeSpan.split('-');
		self.shiftStartTime = tempTimespan[0];
		self.shiftEndTime = tempTimespan[1];
	}
	self.shiftTimeSpan = scheduleDate.Shift ? scheduleDate.Shift.TimeSpan : null;
	self.shiftWorkingHours = scheduleDate.Shift ? scheduleDate.Shift.WorkingHours : null;
	self.shiftColor = scheduleDate.Shift ? scheduleDate.Shift.Color : null;
	self.hasShift = self.shiftName != null;
	self.backgroundColor = scheduleDate.Shift ? scheduleDate.Shift.Color : null;
	self.shiftTextColor = self.backgroundColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor)
		: 'black';

	self.isOutsideMonth = selectedDate.month() != currentDate.month();
	self.currentDayColor =
		moment().year() == currentDate.year() &&
		moment().month() == currentDate.month() &&
		self.dayOfMonth == new Date().getDate()
			? 'red'
			: '';

	function getSeatBookingMessage(seatBookings) {
		var userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();

		var message =
			'<div class="seatbooking-tooltip">' +
			'<span class="tooltip-header">{0}</span><table class="seatbooking-tooltip-table">'.format(
				userTexts.SeatBookings
			);
		var messageEnd = '</table></div>';

		if (seatBookings != null) {
			seatBookings.forEach(function(seatBooking) {
				message += formatSeatBooking(seatBooking);
			});
		}
		message += messageEnd;

		return message;
	}

	function formatSeatBooking(seatBooking) {
		var bookingText = '<tr><td>{0} - {1}</td><td>{2}</td></tr>';

		var fullSeatName = seatBooking.LocationPath != '' ? seatBooking.LocationPath + '/' : '';
		fullSeatName += seatBooking.LocationPrefix || '' + seatBooking.SeatName + seatBooking.LocationSuffix || '';

		return bookingText.format(
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.StartDateTime),
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.EndDateTime),
			fullSeatName
		);
	}
};

Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel = function() {
	this.dayViewModels = ko.observableArray();
};

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
