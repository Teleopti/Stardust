Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent, layoutDirection) {
	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityClass = "probability-none";
	var lowProbabilityClass = "probability-low";
	var highProbabilityClass = "probability-high";
	var expiredProbabilityClass = "probability-expired";


	function generateCssClass() {
		var cssClass = "";
		if (rawProbability.possibility === constants.probabilityLow)
			cssClass = lowProbabilityClass;
		else if (rawProbability.possibility === constants.probabilityHigh)
			cssClass = highProbabilityClass;

		if (parent.userNowInMinute() < 0) {
			return invisibleProbabilityClass;
		} else if (parent.userNowInMinute() < rawProbability.endTimeInMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + expiredProbabilityClass;
		}
	}

	function generateStyleJson() {
		var styleJson = {};
		var startPositionProperty = layoutDirection === constants.horizontalDirectionLayout ? "left" : "top";
		var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";

		styleJson[startPositionProperty] = boundaries.lengthPercentagePerMinute * (rawProbability.startTimeInMinutes - boundaries.timelineStartMinutes) * 100 + "%";
		styleJson[lengthProperty] = boundaries.lengthPercentagePerMinute * (rawProbability.endTimeInMinutes - rawProbability.startTimeInMinutes) * 100 + "%";

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
		if (!(parent.userNowInMinute() >= 0 && parent.userNowInMinute() < rawProbability.endTimeInMinutes))
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
		var startOfToday = moment(rawProbability.startTime).startOf("day");
		var dayDiff = endMoment.diff(startOfToday, "days");
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		return startMoment.minutes(rawProbability.startTimeInMinutes % 60).format(timeFormat) + " - "
			+ endMoment.minutes(rawProbability.endTimeInMinutes % 60).format(timeFormat)
			+ (dayDiff > 0 ? " +" + dayDiff : "");
	}

	return {
		styleJson: generateStyleJson(),
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};