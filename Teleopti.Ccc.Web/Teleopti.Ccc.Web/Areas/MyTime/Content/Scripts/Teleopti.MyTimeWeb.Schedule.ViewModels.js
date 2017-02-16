// Same value as height of class "weekview-day-schedule"
var scheduleHeight = 668;
var pixelToDisplayAll = 38;
var pixelToDisplayTitle = 16;

Teleopti.MyTimeWeb.Schedule.DayViewModel = function (day, probabilities, parent) {
	var self = this;

	self.fixedDate = ko.observable(day.FixedDate);

	self.date = ko.observable(day.Date);
	self.state = ko.observable(day.State);

	var dayDescription = "";
	var dayNumberDisplay = "";
	var dayDate = moment(day.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.headerTitle = ko.observable(dayDate.format("jdddd"));
		self.dayOfWeek = ko.observable(dayDate.weekday());
		dayNumberDisplay = dayDate.jDate();

		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format("jMMMM");
		}
	} else {
		self.headerTitle = ko.observable(day.Header.Title);
		self.dayOfWeek = ko.observable(day.DayOfWeekNumber);
		dayNumberDisplay = dayDate.date();
		if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
			dayDescription = dayDate.format("MMMM");
		}
	}

	self.headerDayDescription = ko.observable(dayDescription);

	self.headerDayNumber = ko.observable(dayNumberDisplay);

	self.textRequestPermission = ko.observable(parent.textPermission());
	self.requestPermission = ko.observable(parent.requestPermission());

	self.summaryStyleClassName = ko.observable(day.Summary.StyleClassName);
	self.summaryTitle = ko.observable(day.Summary.Title);
	self.summaryTimeSpan = ko.observable(day.Summary.TimeSpan);
	self.summary = ko.observable(day.Summary.Summary);
	self.hasOvertime = day.HasOvertime && !day.IsFullDayAbsence;
	self.hasShift = day.Summary.Color !== null ? true : false;
	self.noteMessage = ko.computed(function () {
		//need to html encode due to not bound to "text" in ko
		return $("<div/>").text(day.Note.Message).html();
	});

	self.textRequestCount = ko.observable(day.TextRequestCount);
	self.overtimeAvailability = ko.observable(day.OvertimeAvailabililty);
	self.probabilityClass = ko.observable(day.ProbabilityClass);
	self.probabilityText = ko.observable(day.ProbabilityText);

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

	self.hasNote = ko.observable(day.HasNote);
	self.seatBookings = ko.observableArray(day.SeatBookings);

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
		return parent.styles()[self.summaryStyleClassName()];
	});

	self.textColor = ko.computed(function () {
		var backgroundColor = parent.styles()[self.summaryStyleClassName()];
		if (backgroundColor !== null && backgroundColor !== undefined) {
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return "black";
	});
	self.availability = ko.observable(day.Availability);

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

	var parseTimeSpan = function (timespan) {
		var splitterIndex = timespan.indexOf(" - ");
		var periodEndPosition = splitterIndex + 3;
		var periodStart = timespan.substring(0, splitterIndex);
		var periodEnd = timespan.substring(periodEndPosition, timespan.length);
		var isCrossDayperiod = periodEnd.indexOf("+1") >= 0;

		var baseDate = "2017-01-01 ";
		var startTimeInMinutes = !isCrossDayperiod
			? (moment(baseDate + periodStart).diff(moment(baseDate)) / (60 * 1000))
			: 0;
		var endTimeInMinutes = moment(baseDate + (isCrossDayperiod
				? periodEnd.substring(0, periodEnd.length - "+1".length)
				: periodEnd)).diff(moment(baseDate)) / (60 * 1000);
		return {
			startMinutes: startTimeInMinutes,
			endMinutes: endTimeInMinutes
		};
	}

	var getContinousPeriods = function (periods) {
		if (!periods || periods.length === 0) return [];

		var continousPeriods = [];
		var previousEndTime = 0;
		var continousPeriodStart = 0;
		for (var l = 0; l < periods.length; l++) {
			var currentPeriodTimeSpan = parseTimeSpan(day.Periods[l].TimeSpan);
			var currentPeriodStartTime = currentPeriodTimeSpan.startMinutes;
			var currentPeriodEndTime = currentPeriodTimeSpan.endMinutes;

			if (currentPeriodStartTime === currentPeriodEndTime) continue;

			if (l === 0) {
				continousPeriodStart = currentPeriodStartTime;
			}

			if (previousEndTime !== 0 && currentPeriodStartTime !== previousEndTime) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": previousEndTime
				});
				continousPeriodStart = currentPeriodStartTime;
			}

			if (l === periods.length - 1) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": currentPeriodEndTime
				});
			}
			previousEndTime = currentPeriodEndTime;
		}

		return continousPeriods;
	};

	var createProbabilityModels = function (rawProbabilities) {
		if (rawProbabilities == undefined || rawProbabilities.length === 0) return [];

		// If today is full day absence or dayoff, Then hide absence probabilities
		var probabilityType = parent.probabilityType();
		if (probabilityType === 0 || (probabilityType === 1 && (day.IsFullDayAbsence || day.IsDayOff))) {
			return [];
		}

		var shiftStartMinutes = 0;
		var shiftEndMinutes = 1440; // = 24 * 60, Total minutes of a day
		var shiftStartPosition = 1;
		var shiftEndPosition = 0;

		if (day.IsFullDayAbsence || day.IsDayOff) {
			var timelines = parent.timeLines();
			if (parent.intradayOpenPeriod != null) {
				var openPeriodTimeSpan = parseTimeSpan(parent.intradayOpenPeriod);
				shiftStartMinutes = openPeriodTimeSpan.startMinutes;
				shiftEndMinutes = openPeriodTimeSpan.endMinutes;
			} else {
				shiftStartMinutes = timelines[0].minutes;
				shiftEndMinutes = timelines[timelines.length - 1].minutes;
			}

			// Calculate shiftStartPosition and shiftEndPosition since this could not get from raw data
			var timelineStartMinutes = timelines[0].minutes;
			var timelineEndMinutes = timelines[timelines.length - 1].minutes;

			var heightPercentagePerMinute = 1 / (timelineEndMinutes - timelineStartMinutes);
			shiftStartPosition = (shiftStartMinutes - timelineStartMinutes) * heightPercentagePerMinute;
			shiftEndPosition = (shiftEndMinutes - timelineStartMinutes) * heightPercentagePerMinute;
		} else {
			for (var i = 0; i < day.Periods.length; i++) {
				var period = day.Periods[i];

				var periodTimeSpan = parseTimeSpan(period.TimeSpan);

				if (i === 0) {
					shiftStartMinutes = periodTimeSpan.startMinutes;
				}

				if (i === day.Periods.length - 1) {
					shiftEndMinutes = periodTimeSpan.endMinutes;
				}

				if (shiftStartPosition > period.StartPositionPercentage) {
					shiftStartPosition = period.StartPositionPercentage;
				}
				if (shiftEndPosition < period.EndPositionPercentage) {
					shiftEndPosition = period.EndPositionPercentage;
				}
			}
		}

		var probabilityNames = ["low", "high"];
		var probabilityLabels = [parent.userTexts.low, parent.userTexts.high];
		var probabilities = [];

		// Add an "invisible" probability on top to make all probabilities displayed from correct position
		probabilities.push({
			cssClass: "probability-none",
			tooltips: "",
			styleJson: {
				"top": 0,
				"height": Math.round(scheduleHeight * shiftStartPosition) + "px"
			}
		});

		var totalHeight = shiftEndPosition - shiftStartPosition;

		var continousPeriods = [];
		var tooltipsTitle = "";
		if (probabilityType === 1) {
			tooltipsTitle = parent.userTexts.probabilityForAbsence;
			continousPeriods = getContinousPeriods(day.Periods);
		} else if (probabilityType === 2) {
			tooltipsTitle = parent.userTexts.probabilityForOvertime;
		}

		for (var j = 0; j < rawProbabilities.length; j++) {
			var intervalProbability = rawProbabilities[j];
			var startOfToday = moment(intervalProbability.StartTime).startOf("day");
			var startMoment = moment(intervalProbability.StartTime);
			var endMoment = moment(intervalProbability.EndTime);

			var intervalStartMinutes = startMoment.diff(startOfToday) / (60 * 1000);
			var intervalEndMinutes = endMoment.isSame(startMoment, "day")
				? endMoment.diff(startOfToday) / (60 * 1000)
				: 1439; // 23:59

			var visible = shiftStartMinutes <= intervalStartMinutes && intervalEndMinutes <= shiftEndMinutes;
			if (!visible) continue;

			var inScheduleTimeRange = false;
			if (probabilityType === 1) {
				// Show absence probability within schedule time range only
				for (var m = 0; m < continousPeriods.length; m++) {
					var continousPeriod = continousPeriods[m];
					if (continousPeriod.startTime <= intervalStartMinutes && intervalEndMinutes <= continousPeriod.endTime) {
						inScheduleTimeRange = true;
						break;
					}
				}
			} else {
				inScheduleTimeRange = true;
			}

			var index = intervalProbability.Possibility;
			var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
			var intervalTimeSpan = startMoment.format(timeFormat) + " - " + endMoment.format(timeFormat);
			var tooltips = inScheduleTimeRange
				? "<div style='text-align: center'>" +
				"  <div>" + tooltipsTitle + "</div>" +
				"  <div class='tooltip-wordwrap' style='font-weight: bold'>" + probabilityLabels[index] + "</div>" +
				"  <div class='tooltip-wordwrap' style='overflow: hidden'>" + intervalTimeSpan + "</div>" +
				"</div>"
				: "";

			probabilities.push({
				cssClass: "probability-" + (inScheduleTimeRange ? probabilityNames[index] : "none"),
				tooltips: tooltips
			});
		}

		var heightPerInterval = totalHeight / (probabilities.length - 1);
		for (var k = 1; k < probabilities.length; k++) {
			var topPositionPercentage = shiftStartPosition + heightPerInterval * (k - 1);
			probabilities[k].styleJson = {
				"top": scheduleHeight * topPositionPercentage + "px",
				"height": scheduleHeight * heightPerInterval + "px"
			};
		}

		return probabilities;
	}

	self.probabilities = createProbabilityModels(probabilities);

	self.layers = ko.utils.arrayMap(day.Periods, function (item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, parent.userTexts, self);
	});
};

Teleopti.MyTimeWeb.Schedule.LayerViewModel = function (layer, userTexts, parent) {
	var self = this;

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
		var realTimespan = originalTimespan.length >= 22 ? originalTimespan.replace(" - ", "-").replace(" +1", "+1") : originalTimespan;
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
		return Math.round(scheduleHeight * self.startPositionPercentage());
	});
	self.height = ko.computed(function () {
		var bottom = Math.round(scheduleHeight * self.endPositionPercentage()) + 1;
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
		return scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
	});
	self.showTitle = ko.computed(function () {
		return self.heightDouble() > pixelToDisplayTitle;
	});
	self.showDetail = ko.computed(function () {
		return self.heightDouble() > pixelToDisplayAll;
	});
};