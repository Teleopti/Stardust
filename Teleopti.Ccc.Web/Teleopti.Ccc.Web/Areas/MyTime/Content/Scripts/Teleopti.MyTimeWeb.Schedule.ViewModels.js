﻿Teleopti.MyTimeWeb.Schedule.DayViewModel = function(scheduleDay, parent) {
	var self = this;

	self.fixedDate = ko.observable(scheduleDay.FixedDate);
	self.formattedFixedDate = ko.computed(function() {
		return moment(self.fixedDate()).format('YYYY-MM-DD');
	});
	self.currentUserDate = ko.observable(
		moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(parent.baseUtcOffsetInMinutes)).startOf('day')
	);
	self.formatedCurrentUserDate = ko.computed(function() {
		return moment(self.currentUserDate()).format('YYYY-MM-DD');
	});

	self.date = ko.observable(scheduleDay.Date);
	self.userNowInMinute = ko.observable(-1);
	self.state = ko.observable(scheduleDay.State);
	self.isFullDayAbsence = scheduleDay.IsFullDayAbsence;
	self.isDayOff = ko.observable(scheduleDay.IsDayOff);
	self.periods = scheduleDay.Periods;
	self.openHourPeriod = scheduleDay.OpenHourPeriod;

	var dayDescription = '';
	var dayNumberDisplay = '';
	var dayDate = moment(scheduleDay.FixedDate, Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.headerTitle = ko.observable(dayDate.format('dddd'));
		self.dayOfWeek = ko.observable(dayDate.weekday());
		dayNumberDisplay = dayDate.jDate();

		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format('jMMMM');
		}
	} else {
		self.headerTitle = ko.observable(scheduleDay.Header.Title);
		self.dayOfWeek = ko.observable(scheduleDay.DayOfWeekNumber);
		dayNumberDisplay = dayDate.date();
		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format('MMMM');
		}
	}

	self.headerDayDescription = ko.observable(dayDescription);

	self.headerDayNumber = ko.observable(dayNumberDisplay);

	self.textRequestPermission = ko.observable(parent.textPermission());
	self.requestPermission = ko.observable(parent.requestPermission());

	self.summaryStyleClassName = ko.observable(scheduleDay.Summary.StyleClassName);
	self.summaryTitle = ko.observable(scheduleDay.Summary.Title);
	self.summaryTimeSpan = ko.observable(scheduleDay.Summary.TimeSpan);
	self.summary = ko.observable(scheduleDay.Summary.Summary);
	self.hasOvertime = scheduleDay.HasOvertime && !scheduleDay.IsFullDayAbsence;
	self.hasShift = scheduleDay.Summary.Color !== null ? true : false;

	self.noteMessage = $('<div/>')
		.text(scheduleDay.Note.Message)
		.html(); //need to html encode due to not bound to "text" in ko

	self.requestsCount = ko.observable(scheduleDay.RequestsCount);
	self.overtimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty);

	switch (scheduleDay.ProbabilityClass) {
		case 'poor': {
			self.absenceChanceColor = 'red';
			break;
		}
		case 'fair': {
			self.absenceChanceColor = 'yellow';
			break;
		}
		case 'good': {
			self.absenceChanceColor = 'green';
			break;
		}
		default:
			self.absenceChanceColor = '';
	}

	self.probabilityText = ko.observable(scheduleDay.ProbabilityText);
	self.probabilities = ko.observableArray();

	self.holidayChanceText =
		self.probabilityText() && self.probabilityText().length > 0
			? parent.userTexts.ChanceOfGettingAbsenceRequestGranted + self.probabilityText()
			: '';

	self.hasRequests = ko.computed(function() {
		return self.requestsCount() > 0;
	});

	self.hasNote = ko.observable(scheduleDay.HasNote);
	self.seatBookings = scheduleDay.SeatBookings;

	self.seatBookingIconVisible = ko.computed(function() {
		return self.seatBookings.length > 0;
	});

	var getValueOrEmptyString = function(object) {
		return object || '';
	};

	var formatSeatBooking = function(seatBooking) {
		var bookingText = '<tr><td>{0} - {1}</td><td>{2}</td></tr>';

		var fullSeatName = seatBooking.LocationPath !== '' ? seatBooking.LocationPath + '/' : '';
		fullSeatName +=
			getValueOrEmptyString(seatBooking.LocationPrefix) +
			seatBooking.SeatName +
			getValueOrEmptyString(seatBooking.LocationSuffix);

		return bookingText.format(
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.StartDateTime),
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.EndDateTime),
			fullSeatName
		);
	};

	self.seatBookingMessage = getSeatBookingMessage(self.seatBookings, parent.userTexts);

	self.requestsText = parent.userTexts.XRequests.format(self.requestsCount());

	self.textOvertimeAvailabilityText =
		self.overtimeAvailability().StartTime + ' - ' + self.overtimeAvailability().EndTime;

	self.classForDaySummary = ko.computed(function() {
		var showRequestClass = self.requestPermission()
			? 'weekview-day-summary weekview-day-show-request '
			: 'weekview-day-summary ';
		if (self.summaryStyleClassName() !== null && self.summaryStyleClassName() !== undefined) {
			return showRequestClass + self.summaryStyleClassName(); //last one needs to be becuase of "stripes" and similar
		}
		return showRequestClass; //last one needs to be becuase of "stripes" and similar
	});

	self.colorForDaySummary = ko.computed(function() {
		return parent.styles() && parent.styles()[self.summaryStyleClassName()];
	});

	self.textColor = ko.computed(function() {
		var backgroundColor = parent.styles()[self.summaryStyleClassName()];
		if (backgroundColor !== null && backgroundColor !== undefined) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	});
	self.availability = ko.observable(scheduleDay.Availability);

	self.absenceRequestPermission = ko.computed(function() {
		return parent.absenceRequestPermission() && self.availability();
	});

	self.navigateToRequests = function() {
		parent.navigateToRequestsMethod();
	};

	self.showOvertimeAvailability = function(data) {
		var date = self.fixedDate();
		var momentDate = moment(date, Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);

		if (data.startPositionPercentage === 0 && data.overtimeAvailabilityYesterday) {
			momentDate.add("days", -1);
		}

		date = Teleopti.MyTimeWeb.Common.FormatServiceDate(momentDate);
		var day = ko.utils.arrayFirst(parent.days(), function(item) {
			return item.fixedDate() === date;
		});
		if (day) {
			parent.showAddRequestForm(day);
		} else {
			parent.showAddRequestFormWithData(date, data.overtimeAvailabilityYesterday);
		}
	};

	self.showStaffingProbabilityBar = ko.computed(function() {
		var fixedDateMoment = moment(self.fixedDate());
		var currentUserDate = moment(self.formatedCurrentUserDate());
		if (parent.staffingProbabilityForMultipleDaysEnabled) {
			return (
				fixedDateMoment >= currentUserDate &&
				fixedDateMoment < currentUserDate.add('day', parent.staffingInfoAvailableDays)
			);
		}

		return self.formattedFixedDate() === self.formatedCurrentUserDate();
	});

	self.layers = ko.utils.arrayMap(scheduleDay.Periods, function(item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self, false, 0, true);
	});

	function getSeatBookingMessage(seatBookings, userTexts) {
		var message =
			"<div class='seatbooking-tooltip'>" +
			"<span class='tooltip-header'>{0}</span><table>".format(userTexts.SeatBookings);

		var messageEnd = '</table></div>';

		if (seatBookings !== null) {
			seatBookings.forEach(function(seatBooking) {
				message += formatSeatBooking(seatBooking);
			});
		}
		message += messageEnd;

		return message;
	}
};
