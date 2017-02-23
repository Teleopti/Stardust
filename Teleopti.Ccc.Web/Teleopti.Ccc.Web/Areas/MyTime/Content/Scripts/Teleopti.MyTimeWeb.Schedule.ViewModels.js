/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Content/jalaali-calendar-datepicker/moment-jalaali.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js"/>

Teleopti.MyTimeWeb.Schedule.DayViewModel = function (day, rawProbabilities, parent) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

	self.fixedDate = ko.observable(day.FixedDate);

	self.date = ko.observable(day.Date);
	self.userNowInMinute = ko.observable(-1);
	self.state = ko.observable(day.State);

	self.staffingProbabilityEnabled = ko.observable(parent.staffingProbabilityEnabled());

	var dayDescription = "";
	var dayNumberDisplay = "";
	var dayDate = moment(day.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);

	if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		self.headerTitle = ko.observable(dayDate.format("dddd"));
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

	var convertTimePointToMinutes = function (time) {
		var baseDate = "2017-01-01 ";
		return (moment(baseDate + time).diff(moment(baseDate)) / (60 * 1000));
	}

	var getContinousPeriods = function (date, periods) {
		if (!periods || periods.length === 0) return [];

		var continousPeriods = [];
		var previousEndMinutes = 0;
		var continousPeriodStart = 0;
		for (var l = 0; l < periods.length; l++) {
			var currentPeriodStartMinutes = moment(day.Periods[l].StartTime).diff(date) / (60 * 1000);
			var currentPeriodEndMinutes = moment(day.Periods[l].EndTime).diff(date) / (60 * 1000);

			if (currentPeriodStartMinutes < 0) {
				currentPeriodStartMinutes = 0;
			}

			if (currentPeriodEndMinutes > constants.totalMinutesOfOneDay) {
				currentPeriodEndMinutes = constants.totalMinutesOfOneDay;
			}

			if (currentPeriodStartMinutes === currentPeriodEndMinutes) continue;

			if (l === 0) {
				continousPeriodStart = currentPeriodStartMinutes;
			}

			if (previousEndMinutes !== 0 && currentPeriodStartMinutes !== previousEndMinutes) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": previousEndMinutes
				});
				continousPeriodStart = currentPeriodStartMinutes;
			}

			if (l === periods.length - 1) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": currentPeriodEndMinutes
				});
			}
			previousEndMinutes = currentPeriodEndMinutes;
		}

		return continousPeriods;
	};

	var calculateScheduleAndProbabilityBoundaries = function (scheduleDay, probabilityType, probabilities) {
		var shiftStartMinutes = 0;
		var shiftEndMinutes = constants.totalMinutesOfOneDay;
		var shiftStartPosition = 1;
		var shiftEndPosition = 0;

		var probabilityStartMinutes;
		var probabilityEndMinutes;
		var probabilityStartPosition;
		var probabilityEndPosition;

		var timelines = parent.timeLines();
		var timelineStartMinutes = timelines[0].minutes;
		var timelineEndMinutes = timelines[timelines.length - 1].minutes;
		if (timelineEndMinutes === constants.totalMinutesOfOneDay - 1) {
			timelineEndMinutes = constants.totalMinutesOfOneDay;
		}

		var heightPercentagePerMinute = 1 / (timelineEndMinutes - timelineStartMinutes);

		var momentDate = moment(scheduleDay.FixedDate);
		if (scheduleDay.IsFullDayAbsence || scheduleDay.IsDayOff) {
			if (probabilityType === constants.overtimeProbabilityType &&
				parent.intradayOpenPeriod != undefined && parent.intradayOpenPeriod != null) {
				shiftStartMinutes = convertTimePointToMinutes(parent.intradayOpenPeriod.startTime);
				shiftEndMinutes = convertTimePointToMinutes(parent.intradayOpenPeriod.endTime);
			} else {
				shiftStartMinutes = timelines[0].minutes + constants.intervalLengthInMinutes;
				shiftEndMinutes = timelines[timelines.length - 1].minutes - constants.intervalLengthInMinutes;
			}

			// Calculate shiftStartPosition and shiftEndPosition since this could not get from raw data
			shiftStartPosition = (shiftStartMinutes - timelineStartMinutes) * heightPercentagePerMinute;
			shiftEndPosition = (shiftEndMinutes - timelineStartMinutes) * heightPercentagePerMinute;

			probabilityStartMinutes = shiftStartMinutes;
			probabilityEndMinutes = shiftEndMinutes;
			probabilityStartPosition = shiftStartPosition;
			probabilityEndPosition = shiftEndPosition;
		} else {
			shiftStartMinutes = -1;
			shiftEndMinutes = constants.totalMinutesOfOneDay + 1;
			if (scheduleDay.Periods.length > 0) {
				var firstPeriod = scheduleDay.Periods[0];
				var lastPeriod = scheduleDay.Periods[scheduleDay.Periods.length - 1];

				shiftStartMinutes = moment(firstPeriod.StartTime).diff(momentDate) / (60 * 1000);
				if (shiftStartMinutes < 0) {
					shiftStartMinutes = 0;
				}

				shiftEndMinutes = moment(lastPeriod.EndTime).diff(momentDate) / (60 * 1000);
				if (shiftEndMinutes > constants.totalMinutesOfOneDay) {
					shiftEndMinutes = constants.totalMinutesOfOneDay;
				}
			}

			var rawProbabilityStartMinutes = -1;
			var rawProbabilityEndMinutes = constants.totalMinutesOfOneDay + 1;
			if (probabilities.length > 0) {
				var firstProbabilityStartTime = moment(probabilities[0].StartTime);
				var firstProbabilityStartMinute = (firstProbabilityStartTime.diff(momentDate)) / (60 * 1000);
				if (firstProbabilityStartMinute < 0) {
					firstProbabilityStartMinute = 0;
				}

				if (firstProbabilityStartMinute > rawProbabilityStartMinutes) {
					rawProbabilityStartMinutes = firstProbabilityStartMinute;
				}

				var lastProbabilityEndTime = moment(probabilities[probabilities.length - 1].EndTime);
				var lastProbabilityEndMinute = (lastProbabilityEndTime.diff(momentDate)) / (60 * 1000);
				if (lastProbabilityEndMinute > constants.totalMinutesOfOneDay) {
					lastProbabilityEndMinute = constants.totalMinutesOfOneDay;
				}

				if (rawProbabilityEndMinutes > lastProbabilityEndMinute) {
					rawProbabilityEndMinutes = lastProbabilityEndMinute;
				}
			}

			var openPeriodStartMinutes = -1;
			var openPeriodEndMinutes = constants.totalMinutesOfOneDay + 1;
			if (probabilityType === constants.overtimeProbabilityType &&
				parent.intradayOpenPeriod != undefined &&
				parent.intradayOpenPeriod != null) {
				openPeriodStartMinutes = convertTimePointToMinutes(parent.intradayOpenPeriod.startTime);
				openPeriodEndMinutes = convertTimePointToMinutes(parent.intradayOpenPeriod.endTime);
			}

			var startTimeCandidates = [
				rawProbabilityStartMinutes, timelineStartMinutes + constants.intervalLengthInMinutes
			];
			var endTimeCandidates = [
				rawProbabilityEndMinutes, timelineEndMinutes - constants.intervalLengthInMinutes
			];

			if (probabilityType === constants.absenceProbabilityType) {
				startTimeCandidates.push(shiftStartMinutes);
				endTimeCandidates.push(shiftEndMinutes);
			} else if (probabilityType === constants.overtimeProbabilityType) {
				startTimeCandidates.push(openPeriodStartMinutes);
				endTimeCandidates.push(openPeriodEndMinutes);
			}

			probabilityStartMinutes = Math.max.apply(null, startTimeCandidates);
			probabilityEndMinutes = Math.min.apply(null, endTimeCandidates);

			probabilityStartPosition = (probabilityStartMinutes - timelineStartMinutes) * heightPercentagePerMinute;
			probabilityEndPosition = (probabilityEndMinutes - timelineStartMinutes) * heightPercentagePerMinute;
		}

		return {
			heightPercentagePerMinute: heightPercentagePerMinute,
			shiftStartMinutes: shiftStartMinutes,
			shiftEndMinutes: shiftEndMinutes,
			shiftStartPosition: shiftStartPosition,
			shiftEndPosition: shiftEndPosition,
			probabilityStartMinutes: probabilityStartMinutes,
			probabilityEndMinutes: probabilityEndMinutes,
			probabilityStartPosition: probabilityStartPosition,
			probabilityEndPosition: probabilityEndPosition
		};
	}

	var createProbabilityModel = function (rawProbability, probabilityType, boundaries, continousPeriods, tooltipsTitle, heightPerInterval) {
		var probabilityNames = ["low", "high"];
		var probabilityLabels = [parent.userTexts.low, parent.userTexts.high];

		var startOfToday = moment(rawProbability.StartTime).startOf("day");
		var startMoment = moment(rawProbability.StartTime);
		var endMoment = moment(rawProbability.EndTime);

		var intervalStartMinutes = startMoment.diff(startOfToday) / (60 * 1000);
		var intervalEndMinutes = endMoment.isSame(startMoment, "day")
			? endMoment.diff(startOfToday) / (60 * 1000)
			: constants.totalMinutesOfOneDay - 1;

		var shouldGenerateViewModel = boundaries.probabilityStartMinutes <= intervalStartMinutes && intervalEndMinutes <= boundaries.probabilityEndMinutes;
		if (!shouldGenerateViewModel) return undefined;

		var visible = false;
		if (probabilityType === constants.absenceProbabilityType) {
			// Show absence probability within schedule time range only
			for (var m = 0; m < continousPeriods.length; m++) {
				var continousPeriod = continousPeriods[m];
				if (continousPeriod.startTime <= intervalStartMinutes && intervalEndMinutes <= continousPeriod.endTime) {
					visible = true;
					break;
				}
			}
		} else if (probabilityType === constants.overtimeProbabilityType) {
			visible = boundaries.probabilityStartMinutes <= intervalStartMinutes && intervalEndMinutes <= boundaries.probabilityEndMinutes;;
		}

		var index = rawProbability.Possibility;
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		var intervalTimeSpan = startMoment.format(timeFormat) + " - " + endMoment.format(timeFormat);

		var tooltips = "";
		var cssClass = "probability-none";
		if (visible) {
			cssClass = "probability-" + probabilityNames[index];
			tooltips = "<div style='text-align: center'>" +
			"  <div>" + tooltipsTitle + "</div>" +
			"  <div class='tooltip-wordwrap' style='font-weight: bold'>" + probabilityLabels[index] + "</div>" +
			"  <div class='tooltip-wordwrap' style='overflow: hidden'>" + intervalTimeSpan + "</div>" +
			"</div>";
		}

		return {
			startMinutes: intervalStartMinutes,
			endInMinutes: intervalEndMinutes,
			actualClass: cssClass,
			actualTooltips: tooltips,
			styleJson: { "height": constants.scheduleHeight * heightPerInterval + "px" },
			cssClass: function () {
				return (self.userNowInMinute() >= 0 && self.userNowInMinute() < this.endInMinutes)
					? this.actualClass
					: "probability-none";
			},
			tooltips: function () {
				return (self.userNowInMinute() >= 0 && self.userNowInMinute() < intervalEndMinutes)
					? tooltips
					: "";
			}
		};
	}

	var createProbabilityModels = function (rawProbabilities) {
		if (!self.staffingProbabilityEnabled() || rawProbabilities == undefined || rawProbabilities.length === 0) {
			return [];
		}

		// If today is full day absence or dayoff, Then hide absence probabilities
		var probabilityType = parent.probabilityType();
		if (probabilityType === constants.noneProbabilityType ||
			(probabilityType === constants.absenceProbabilityType && (day.IsFullDayAbsence || day.IsDayOff))) {
			return [];
		}

		var continousPeriods = [];
		var tooltipsTitle = "";

		var date = moment(day.FixedDate);
		if (probabilityType === constants.absenceProbabilityType) {
			tooltipsTitle = parent.userTexts.probabilityForAbsence;
			continousPeriods = getContinousPeriods(date, day.Periods);
		} else if (probabilityType === constants.overtimeProbabilityType) {
			tooltipsTitle = parent.userTexts.probabilityForOvertime;
		}

		var boundaries = calculateScheduleAndProbabilityBoundaries(day, probabilityType, rawProbabilities);

		var probabilitieModels = [];
		// Add an "invisible" probability on top to make all probabilities displayed from correct position
		probabilitieModels.push({
			actualClass: "probability-none",
			actualTooltips: "",
			cssClass: function () { return "probability-none"; },
			tooltips: function () { return "" },
			styleJson: { "height": Math.round(constants.scheduleHeight * boundaries.probabilityStartPosition) + "px" }
		});

		var heightPerInterval = boundaries.heightPercentagePerMinute * constants.intervalLengthInMinutes;

		for (var j = 0; j < rawProbabilities.length; j++) {
			var probabilityModel = createProbabilityModel(rawProbabilities[j], probabilityType, boundaries,
				continousPeriods, tooltipsTitle, heightPerInterval);
			if (probabilityModel != undefined) {
				probabilitieModels.push(probabilityModel);
			}
		}

		//return probabilitieModels.length > 1 ? probabilitieModels : [];
		return probabilitieModels;
	}

	self.probabilities = createProbabilityModels(rawProbabilities);

	self.layers = ko.utils.arrayMap(day.Periods, function (item) {
		return new Teleopti.MyTimeWeb.Schedule.LayerViewModel(item, parent.userTexts, self);
	});
};

Teleopti.MyTimeWeb.Schedule.LayerViewModel = function (layer, userTexts, parent) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

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