/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Content/jalaali-calendar-datepicker/moment-jalaali.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityModels.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.LayerViewModel.js" />

Teleopti.MyTimeWeb.Schedule.DayViewModel = function (scheduleDay, parent) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.fixedDate = ko.observable(scheduleDay.FixedDate);
	self.formattedFixedDate = ko.computed(function () {
		return moment(self.fixedDate()).format("YYYY-MM-DD");
	});
	self.currentUserDate = ko.observable(moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime()).startOf("day"));
	self.formatedCurrentUserDate = ko.computed(function () {
		return moment(self.currentUserDate()).format("YYYY-MM-DD");
	});

	self.date = ko.observable(scheduleDay.Date);
	self.userNowInMinute = ko.observable(-1);
	self.state = ko.observable(scheduleDay.State);
	self.isFullDayAbsence = scheduleDay.IsFullDayAbsence;
	self.isDayOff = scheduleDay.IsDayOff;
	self.periods = scheduleDay.Periods;
	self.siteOpenHourPeriod = scheduleDay.SiteOpenHourPeriod;

	var dayDescription = "";
	var dayNumberDisplay = "";
	var dayDate = moment(scheduleDay.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.headerTitle = ko.observable(dayDate.format("dddd"));
		self.dayOfWeek = ko.observable(dayDate.weekday());
		dayNumberDisplay = dayDate.jDate();

		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format("jMMMM");
		}
	} else {
		self.headerTitle = ko.observable(scheduleDay.Header.Title);
		self.dayOfWeek = ko.observable(scheduleDay.DayOfWeekNumber);
		dayNumberDisplay = dayDate.date();
		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format("MMMM");
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
	self.noteMessage = ko.computed(function () {
		//need to html encode due to not bound to "text" in ko
		return $("<div/>").text(scheduleDay.Note.Message).html();
	});

	self.textRequestCount = ko.observable(scheduleDay.TextRequestCount);
	self.overtimeAvailability = ko.observable(scheduleDay.OvertimeAvailabililty);
	self.probabilityClass = ko.observable(scheduleDay.ProbabilityClass);
	self.probabilityText = ko.observable(scheduleDay.ProbabilityText);
	self.probabilities = ko.observableArray();

	self.holidayChanceText = ko.computed(function () {
		var probabilityText = self.probabilityText();
		if (probabilityText)
			return parent.userTexts.ChanceOfGettingAbsencerequestGranted + probabilityText;
		return probabilityText;
	});

	self.holidayChanceColor = ko.computed(function () {
		return self.probabilityClass();
	});

	self.hasTextRequest = ko.computed(function () {
		return self.textRequestCount() > 0;
	});

	self.hasNote = ko.observable(scheduleDay.HasNote);
	self.seatBookings = ko.observableArray(scheduleDay.SeatBookings);

	self.seatBookingIconVisible = ko.computed(function () {
		return self.seatBookings().length > 0;
	});

	var getValueOrEmptyString = function (object) {
		return object || "";
	}

	var formatSeatBooking = function (seatBooking) {
		var bookingText = "<tr><td>{0} - {1}</td><td>{2}</td></tr>";

		var fullSeatName = seatBooking.LocationPath !== "" ? seatBooking.LocationPath + "/" : "";
		fullSeatName += getValueOrEmptyString(seatBooking.LocationPrefix) + seatBooking.SeatName + getValueOrEmptyString(seatBooking.LocationSuffix);

		return bookingText.format(
				Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.StartDateTime),
				Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.EndDateTime),
				fullSeatName
				);
	};

	self.seatBookingMessage = ko.computed(function () {
		var message = "<div class='seatbooking-tooltip'>" +
							"<span class='tooltip-header'>{0}</span><table>".format(parent.userTexts.SeatBookingsTitle);

		var messageEnd = "</table></div>";

		if (self.seatBookings() !== null) {
			self.seatBookings().forEach(function (seatBooking) {
				message += formatSeatBooking(seatBooking);
			});
		}
		message += messageEnd;

		return message;
	});

	self.textRequestText = ko.computed(function () {
		return parent.userTexts.XRequests.format(self.textRequestCount());
	});

	self.textOvertimeAvailabilityText = ko.computed(function () {
		return self.overtimeAvailability().StartTime + " - " + self.overtimeAvailability().EndTime;
	});

	self.classForDaySummary = ko.computed(function () {
		var showRequestClass = self.requestPermission() ? "weekview-day-summary weekview-day-show-request " : "weekview-day-summary ";
		if (self.summaryStyleClassName() !== null && self.summaryStyleClassName() !== undefined) {
			return showRequestClass + self.summaryStyleClassName(); //last one needs to be becuase of "stripes" and similar
		}
		return showRequestClass; //last one needs to be becuase of "stripes" and similar
	});

	self.colorForDaySummary = ko.computed(function () {
		return parent.styles() && parent.styles()[self.summaryStyleClassName()];
	});

	self.textColor = ko.computed(function () {
		var backgroundColor = parent.styles()[self.summaryStyleClassName()];
		if (backgroundColor !== null && backgroundColor !== undefined) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return "black";
	});
	self.availability = ko.observable(scheduleDay.Availability);

	self.absenceRequestPermission = ko.computed(function () {
		return (parent.absenceRequestPermission() && self.availability());
	});

	self.navigateToRequests = function () {
		parent.navigateToRequestsMethod();
	};

	self.showOvertimeAvailability = function (data) {
		var date = self.fixedDate();
		var momentDate = moment(date, Teleopti.MyTimeWeb.Common.ServiceDateFormat);
		if (data.startPositionPercentage() === 0 && data.overtimeAvailabilityYesterday) {
			momentDate.add("days", -1);
		}
		date = Teleopti.MyTimeWeb.Common.FormatServiceDate(momentDate);
		var day = ko.utils.arrayFirst(parent.days(), function (item) {
			return item.fixedDate() === date;
		});
		if (day) {
			parent.showAddRequestForm(day);
		} else {
			parent.showAddRequestFormWithData(date, data.overtimeAvailabilityYesterday);
		}
	};

	self.showStaffingProbabilityBar = ko.computed(function () {
		if(parent.staffingProbabilityForMultipleDaysEnabled)
			return  (moment(self.fixedDate()) >= moment(self.formatedCurrentUserDate())) && (moment(self.fixedDate()) < moment(self.formatedCurrentUserDate()).add('day', constants.maximumDaysDisplayingProbability));

		return self.formattedFixedDate() === self.formatedCurrentUserDate();
	});

	self.layers = ko.utils.arrayMap(scheduleDay.Periods, function (item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, self);
	});
};
