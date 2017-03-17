Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent, layoutDirection) {
	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityClass = "probability-none";
	var lowProbabilityClass = "probability-low";
	var highProbabilityClass = "probability-high";
	var expiredProbabilityClass = "probability-expired";

	var startOfToday = moment(rawProbability.startTime).startOf("day");
	var intervalStartMinutes = rawProbability.startTimeInMinutes;
	var intervalEndMinutes = rawProbability.endTimeInMinutes;
	var startDiffInMin = 0, endDiffInMin = 0;

	function trimIntervalAccordingSchedulePeriod() {
		if (probabilityType === constants.absenceProbabilityType) {
			for (var i = 0; i < continousPeriods.length; i++) {
				var continousPeriod = continousPeriods[i];
				if ((intervalStartMinutes <= continousPeriod.startTimeInMin && intervalEndMinutes >= continousPeriod.startTimeInMin)) {
					startDiffInMin = continousPeriod.startTimeInMin - intervalStartMinutes;
					intervalStartMinutes = continousPeriod.startTimeInMin;
				}

				if ((intervalStartMinutes <= continousPeriod.endTimeInMin && intervalEndMinutes >= continousPeriod.endTimeInMin)) {
					endDiffInMin = intervalEndMinutes - continousPeriod.endTimeInMin;
					intervalEndMinutes = continousPeriod.endTimeInMin;
				}
			}
		}
	}

	function generateCssClass() {
		trimIntervalAccordingSchedulePeriod();

		var cssClass = "";
		if (rawProbability.possibility === constants.probabilityLow)
			cssClass = lowProbabilityClass;
		else if (rawProbability.possibility === constants.probabilityHigh)
			cssClass = highProbabilityClass;

		if (parent.userNowInMinute() < 0) {
			return invisibleProbabilityClass;
		} else if (parent.userNowInMinute() < intervalEndMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + expiredProbabilityClass;
		}
	}

	function generateStyleJson() {
		trimIntervalAccordingSchedulePeriod();

		var styleJson = {};
		var startPositionProperty = layoutDirection === constants.horizontalDirectionLayout ? "left" : "top";
		var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";

		styleJson[startPositionProperty] = boundaries.lengthPercentagePerMinute * (intervalStartMinutes - boundaries.timelineStartMinutes) * 100 + "%";
		styleJson[lengthProperty] = boundaries.lengthPercentagePerMinute * (intervalEndMinutes - intervalStartMinutes) * 100 + "%";

		return styleJson;
	}

	function getTooltipsTitle() {
		var result = "";
		if (probabilityType === constants.absenceProbabilityType) {
			result = userTexts.probabilityForAbsence;
		} else if (probabilityType === constants.overtimeProbabilityType) {
			result = userTexts.probabilityForOvertime;
		}
		return result;
	}

	function generateTooltips() {
		trimIntervalAccordingSchedulePeriod();

		if (!(parent.userNowInMinute() >= 0 && parent.userNowInMinute() < intervalEndMinutes))
			return "";

		var label = "",
			tooltipTitle = getTooltipsTitle(),
			intervalTimeSpanText = generateIntervalTimeSpanText(rawProbability.startTime, rawProbability.endTime);

		if (rawProbability.possibility === constants.probabilityLow)
			label = userTexts.low;
		else if (rawProbability.possibility === constants.probabilityHigh)
			label = userTexts.high;

		return "<div>" +
			"  <div>" + tooltipTitle + "</div>" +
			"  <div class='tooltip-wordwrap'>" + label + "</div>" +
			"  <div class='tooltip-wordwrap'>" + intervalTimeSpanText + "</div>" +
			"</div>";
	}

	function generateIntervalTimeSpanText(startMoment, endMoment) {
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		var dayDiff = endMoment.diff(startOfToday, "days");
		return startMoment.minutes(intervalStartMinutes % 60).format(timeFormat) + " - " + endMoment.minutes(intervalEndMinutes % 60).format(timeFormat) + (dayDiff > 0 ? " +" + dayDiff : "");
	}

	return {
		styleJson: generateStyleJson(),
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};