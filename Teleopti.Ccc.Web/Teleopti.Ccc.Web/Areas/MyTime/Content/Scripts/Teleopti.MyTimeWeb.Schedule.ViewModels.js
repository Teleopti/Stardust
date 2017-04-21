/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Content/jalaali-calendar-datepicker/moment-jalaali.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ProbabilityModels.js" />

Teleopti.MyTimeWeb.Schedule.DayViewModel = function (scheduleDay, rawProbabilities, parent) {
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
	self.mergeIdenticalProbabilityIntervals = false;
	self.hideProbabilityEarlierThanNow = true;
	self.probabilities = ko.observableArray();

	self.holidayChanceText = ko.computed(function () {
		var probabilityText = self.probabilityText();
		if (probabilityText)
			return parent.userTexts.chanceOfGettingAbsencerequestGranted + probabilityText;
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
							"<span class='tooltip-header'>{0}</span><table>".format(parent.userTexts.seatBookingsTitle);

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
		return parent.userTexts.xRequests.format(self.textRequestCount());
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

	self.showProbabilityBar = ko.computed(function () {
		if(parent.staffingProbabilityForMultipleDaysEnabled())
			return  (moment(self.fixedDate()) >= moment(self.formatedCurrentUserDate())) && (moment(self.fixedDate()) < moment(self.formatedCurrentUserDate()).add('day', constants.maximumDaysDisplayingProbability));
		return self.formattedFixedDate() === self.formatedCurrentUserDate();
	});

	if (self.showProbabilityBar()) {
		self.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(scheduleDay, rawProbabilities, self,
		{
			probabilityType: parent.probabilityType(),
			layoutDirection: constants.layoutDirection.vertical,
			timelines: parent.timeLines(),
			intradayOpenPeriod: parent.intradayOpenPeriod,
			mergeIntervals: self.mergeIdenticalProbabilityIntervals,
			hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
			userTexts: parent.userTexts
		}));
	}

	self.layers = ko.utils.arrayMap(scheduleDay.Periods, function (item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, parent.userTexts, self);
	});
};

Teleopti.MyTimeWeb.Schedule.LayerViewModel = function (layer, userTexts, parent) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	self.title = ko.observable(layer.Title);
	self.hasMeeting = ko.computed(function () {
		return layer.Meeting !== null;
	});
	self.meetingTitle = ko.computed(function () {
		if (self.hasMeeting()) {
			return layer.Meeting.Title;
		}
		return null;
	});
	self.meetingLocation = ko.computed(function () {
		if (self.hasMeeting()) {
			return layer.Meeting.Location;
		}
		return null;
	});
	self.meetingDescription = ko.computed(function () {
		if (self.hasMeeting()) {
			if (layer.Meeting.Description.length > 300) {
				return layer.Meeting.Description.substring(0, 300) + "...";
			}
			return layer.Meeting.Description;
		}
		return null;
	});

	self.timeSpan = ko.computed(function () {
		var originalTimespan = layer.TimeSpan;
		// Remove extra space for extreme long timespan (For example: "10:00 PM - 12:00 AM +1")
		var realTimespan = originalTimespan.length >= 22
			? originalTimespan.replace(" - ", "-").replace(" +1", "+1")
			: originalTimespan;
		return realTimespan;
	});

	self.tooltipText = ko.computed(function () {
		var tooltipContent = "<div>{0}</div>".format(self.title());
		if (!self.hasMeeting()) {
			return tooltipContent + self.timeSpan();
		}

		var text = ("<div>{0}</div><div style='text-align: left'>" +
				"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{1}</i> {2}</div>" +
				"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{3}</i> {4}</div>" +
				"<div class='tooltip-wordwrap' style='white-space: normal'><i>{5}</i> {6}</div>" +
				"</div>")
			.format(self.timeSpan(),
				userTexts.subjectColon,
				$("<div/>").text(self.meetingTitle()).html(),
				userTexts.locationColon,
				$("<div/>").text(self.meetingLocation()).html(),
				userTexts.descriptionColon,
				$("<div/>").text(self.meetingDescription()).html());
		return tooltipContent + text;
	});

	self.backgroundColor = ko.observable("rgb(" + layer.Color + ")");
	self.textColor = ko.computed(function () {
		if (layer.Color !== null && layer.Color !== undefined) {
			var backgroundColor = "rgb(" + layer.Color + ")";
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return "black";
	});

	self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
	self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
	self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
	self.isOvertimeAvailability = ko.observable(layer.IsOvertimeAvailability);
	self.isOvertime = layer.IsOvertime;

	self.top = ko.computed(function () {
		return Math.round(constants.scheduleHeight * self.startPositionPercentage());
	});
	self.height = ko.computed(function () {
		var bottom = Math.round(constants.scheduleHeight * self.endPositionPercentage()) + 1;
		var top = self.top();
		return bottom > top ? bottom - top : 0;
	});
	self.topPx = ko.computed(function () {
		return self.top() + "px";
	});
	self.widthPx = ko.computed(function () {
		var width;
		if (layer.IsOvertimeAvailability) {
			width = 20;
		} else if (parent.probabilities && parent.probabilities.length > 0) {
			width = 115;
		} else {
			width = 127;
		}
		return width + "px";
	});
	self.heightPx = ko.computed(function () {
		return self.height() + "px";
	});
	self.overTimeLighterBackgroundStyle = ko.computed(function () {
		var rgbTohex = function (rgb) {
			if (rgb.charAt(0) === "#")
				return rgb;
			var ds = rgb.split(/\D+/);
			var decimal = Number(ds[1]) * 65536 + Number(ds[2]) * 256 + Number(ds[3]);
			var digits = 6;
			var hexString = decimal.toString(16);
			while (hexString.length < digits)
				hexString += "0";

			return "#" + hexString;
		}

		var getLumi = function (cstring) {
			var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
			if (!matched) return null;
			return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
		}

		var lightColor = "#00ffff";
		var darkColor = "#795548";
		var backgroundColor = rgbTohex(self.backgroundColor());
		var useLighterStyle = Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) > Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

		return useLighterStyle;
	});

	self.overTimeDarkerBackgroundStyle = ko.computed(function () { return !self.overTimeLighterBackgroundStyle(); });

	self.styleJson = ko.computed(function () {
		return {
			"top": self.topPx,
			"width": self.widthPx,
			"height": self.heightPx,
			"color": self.textColor,
			"background-size": self.isOvertime ? "11px 11px" : "initial",
			"background-color": self.backgroundColor
		};
	});

	self.heightDouble = ko.computed(function () {
		return constants.scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
	});
	self.showTitle = ko.computed(function () {
		return self.heightDouble() > constants.pixelToDisplayTitle;
	});
	self.showDetail = ko.computed(function () {
		return self.heightDouble() > constants.pixelToDisplayAll;
	});
};