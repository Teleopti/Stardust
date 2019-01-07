Teleopti.MyTimeWeb.Schedule.DayViewModel = function(scheduleDay, parent) {
	var self = this,
		fixedDateMoment = moment(
			scheduleDay.FixedDate,
			Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly
		);

	self.fixedDate = scheduleDay.FixedDate;
	self.notPastTime = self.fixedDate >= parent.currentUserDate();

	self.userNowInMinute = -1;
	self.state = scheduleDay.State;
	self.isFullDayAbsence = scheduleDay.IsFullDayAbsence;
	self.isDayOff = scheduleDay.IsDayOff;
	self.periods = scheduleDay.Periods;
	self.openHourPeriod = scheduleDay.OpenHourPeriod;
	self.headerDayDescription = '';

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.headerTitle = fixedDateMoment.format('dddd');
		self.dayOfWeek = fixedDateMoment.weekday();
		self.headerDayNumber = fixedDateMoment.jDate();

		if (self.headerDayNumber === 1 || self.dayOfWeek === parent.weekStart) {
			self.headerDayDescription = fixedDateMoment.format('jMMMM');
		}
	} else {
		self.headerTitle = scheduleDay.Header.Title;
		self.dayOfWeek = scheduleDay.DayOfWeekNumber;
		self.headerDayNumber = fixedDateMoment.date();
		if (self.headerDayNumber === 1 || self.dayOfWeek === parent.weekStart) {
			self.headerDayDescription = fixedDateMoment.format('MMMM');
		}
	}

	self.textRequestPermission = parent.textPermission();
	self.requestPermission = parent.requestPermission();
	self.summaryStyleClassName = scheduleDay.Summary.StyleClassName;
	self.summaryTitle = scheduleDay.Summary.Title;
	self.summaryTimeSpan = scheduleDay.Summary.TimeSpan;
	self.summary = scheduleDay.Summary.Summary;
	self.hasOvertime = scheduleDay.HasOvertime && !scheduleDay.IsFullDayAbsence;
	self.hasShift = scheduleDay.Summary.Color !== null ? true : false;

	//need to html encode due to not bound to "text" in ko
	self.noteMessage = $('<div/>')
		.text(scheduleDay.Note.Message)
		.html();

	self.requestsCount = scheduleDay.RequestsCount;
	self.overtimeAvailability = scheduleDay.OvertimeAvailabililty;

	self.absenceChanceColor = getTrafficLightColor(scheduleDay.ProbabilityClass);
	self.trafficLightClass = getNewTrafficLightCssClass(scheduleDay.ProbabilityClass);
	self.probabilityText = scheduleDay.ProbabilityText;
	self.probabilities = ko.observableArray();

	self.hasRequests = ko.observable(self.requestsCount > 0); //use observable because this can be updated in increaseRequestCount
	self.hasNote = scheduleDay.HasNote;
	self.seatBookings = scheduleDay.SeatBookings;
	self.seatBookingIconVisible = self.seatBookings.length > 0;

	self.seatBookingMessage = '';
	self.requestsText = '';
	self.holidayChanceText = '';

	if (parent && parent.userTexts) {
		self.seatBookingMessage = getSeatBookingMessage(self.seatBookings, parent.userTexts);
		self.requestsText = (parent.userTexts.XRequests || '').format(self.requestsCount);
		self.holidayChanceText =
			self.probabilityText && self.probabilityText.length > 0
				? parent.userTexts.ChanceOfGettingAbsenceRequestGranted + self.probabilityText
				: '';
	}

	self.textOvertimeAvailabilityText = self.overtimeAvailability.StartTime + ' - ' + self.overtimeAvailability.EndTime;

	self.classForDaySummary = getClassForDaySummary(self.requestPermission, self.summaryStyleClassName);

	self.colorForDaySummary = parent.styles() && parent.styles()[self.summaryStyleClassName];

	self.textColor = getTextColor(self.summaryStyleClassName);
	self.availability = scheduleDay.Availability;

	self.absenceRequestPermission = parent.absenceRequestPermission() && self.availability;

	self.navigateToRequests = function() {
		parent.navigateToRequestsMethod();
	};

	self.showOvertimeAvailability = function(data) {
		var date = self.fixedDate;
		var momentDate = moment(date, Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);

		if (data.startPositionPercentage === 0 && data.overtimeAvailabilityYesterday) {
			momentDate.add('days', -1);
		}

		date = Teleopti.MyTimeWeb.Common.FormatServiceDate(momentDate);
		var day = ko.utils.arrayFirst(parent.days(), function(item) {
			return item.fixedDate === date;
		});
		if (day) {
			parent.showAddRequestForm(day);
		} else {
			parent.showAddRequestFormWithData(date, data.overtimeAvailabilityYesterday);
		}
	};

	self.showStaffingProbabilityBar = showStaffingProbabilityBar(self.fixedDate, parent);

	self.layers = scheduleDay.Periods.map(function(item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self, false, 0, true);
	});

	function getTrafficLightColor(probabilityClass) {
		switch (probabilityClass) {
			case 'poor': {
				return 'red';
			}
			case 'fair': {
				return 'yellow';
			}
			case 'good': {
				return 'green';
			}
			default:
				return '';
		}
	}

	function getNewTrafficLightCssClass(trafficLightClass) {
		switch (trafficLightClass) {
			case 'poor': {
				return 'traffic-light-progress-poor';
			}
			case 'fair': {
				return 'traffic-light-progress-fair';
			}
			case 'good': {
				return 'traffic-light-progress-good';
			}
			default:
				return '';
		}
	}

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

	function formatSeatBooking(seatBooking) {
		var bookingText = '<tr><td>{0} - {1}</td><td>{2}</td></tr>';

		var fullSeatName = seatBooking.LocationPath !== '' ? seatBooking.LocationPath + '/' : '';
		fullSeatName += seatBooking.LocationPrefix || '' + seatBooking.SeatName + seatBooking.LocationSuffix || '';

		return bookingText.format(
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.StartDateTime),
			Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.EndDateTime),
			fullSeatName
		);
	}

	function getClassForDaySummary(requestPermission, summaryStyleClassName) {
		var showRequestClass = requestPermission
			? 'weekview-day-summary weekview-day-show-request '
			: 'weekview-day-summary ';
		if (summaryStyleClassName !== null && summaryStyleClassName !== undefined) {
			//last one needs to be becuase of "stripes" and similar
			return showRequestClass + summaryStyleClassName;
		}
		return showRequestClass; //last one needs to be becuase of "stripes" and similar
	}

	function getTextColor(summaryStyleClassName) {
		var backgroundColor = parent.styles()[summaryStyleClassName];
		if (backgroundColor !== null && backgroundColor !== undefined) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	}

	function showStaffingProbabilityBar(fixedDate, parent) {
		var formattedCurrentUserDate = moment(
			Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(parent.baseUtcOffsetInMinutes)
		)
			.startOf('day')
			.format('YYYY-MM-DD');

		if (parent.staffingProbabilityForMultipleDaysEnabled) {
			var date = moment(fixedDate);
			var currentUserDate = moment(formattedCurrentUserDate);
			return date >= currentUserDate && date < currentUserDate.add('day', parent.staffingInfoAvailableDays);
		}

		return moment(fixedDate).format('YYYY-MM-DD') === formattedCurrentUserDate;
	}
};
