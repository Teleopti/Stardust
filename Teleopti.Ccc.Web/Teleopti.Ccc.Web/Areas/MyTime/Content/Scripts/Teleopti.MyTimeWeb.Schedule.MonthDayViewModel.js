Teleopti.MyTimeWeb.Schedule.MonthDayViewModel = function(scheduleData, selectedDate) {
	var self = this;

	var currentDate = moment(scheduleData.FixedDate, 'YYYY-MM-DD');
	self.currentDate = currentDate;
	self.date = scheduleData.FixedDate;

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.dayOfMonth = currentDate.jDate();
		self.dayOfWeek = currentDate.format('dddd');
		self.isOutsideMonth = selectedDate.jMonth() != currentDate.jMonth();
	} else {
		self.dayOfMonth = currentDate.date();
		self.dayOfWeek = currentDate.format('dddd');
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

	self.absenceName = scheduleData.Absences ? scheduleData.Absences[0].Name : null;
	self.absenceShortName = scheduleData.Absences ? scheduleData.Absences[0].ShortName : null;
	self.absenceColor = scheduleData.Absences ? scheduleData.Absences[0].Color : '';
	self.absenceTextColor = self.absenceColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.absenceColor)
		: 'black';
	self.hasAbsence = self.absenceName != null;
	self.hasMultipleAbsences = scheduleData.Absences && scheduleData.Absences.length > 1;
	self.isFullDayAbsence = scheduleData.Absences ? scheduleData.Absences[0].IsFullDayAbsence : null;
	self.hasOvertime = scheduleData.Overtimes != null;
	self.hasMultipleOvertimes = scheduleData.Overtimes && scheduleData.Overtimes.length > 1;
	self.overtimeColor = scheduleData.Overtimes ? scheduleData.Overtimes[0].Color : '';
	self.overtimeTextColor = self.overtimeColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.overtimeColor)
		: 'black';
	self.hasSeatBooking = scheduleData.SeatBookings && scheduleData.SeatBookings.length > 0;
	self.seatBookings = scheduleData.SeatBookings;
	self.isDayOff = scheduleData.IsDayOff;
	self.shiftName = scheduleData.Shift ? scheduleData.Shift.Name : null;
	self.shiftShortName = scheduleData.Shift ? scheduleData.Shift.ShortName : null;
	if (scheduleData.Shift && scheduleData.Shift.TimeSpan) {
		var tempTimespan = scheduleData.Shift.TimeSpan.split('-');
		self.shiftStartTime = tempTimespan[0];
		self.shiftEndTime = tempTimespan[1];
	}
	self.shiftTimeSpan = scheduleData.Shift ? scheduleData.Shift.TimeSpan : null;
	self.shiftWorkingHours = scheduleData.Shift ? scheduleData.Shift.WorkingHours : null;
	self.shiftColor = scheduleData.Shift ? scheduleData.Shift.Color : null;
	self.hasShift = self.shiftName != null;
	self.backgroundColor = scheduleData.Shift ? scheduleData.Shift.Color : null;
	self.shiftTextColor = self.backgroundColor
		? Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor)
		: 'black';

	self.bankHoliday = null;
	if (scheduleData.BankHolidayCalendar) {
		var bankHolidayCalendar = scheduleData.BankHolidayCalendar;
		self.bankHoliday = {
			calendarId: bankHolidayCalendar.CalendarId,
			calendarName: bankHolidayCalendar.CalendarName,
			dateDescription: bankHolidayCalendar.DateDescription
		};
	}

	self.isOutsideMonth = selectedDate.month() != currentDate.month();
	self.isCurrentDay =
		moment().year() == currentDate.year() &&
		moment().month() == currentDate.month() &&
		self.dayOfMonth == new Date().getDate();
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
