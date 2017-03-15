Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent, layoutDirection) {

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityClass = "probability-none";
	var lowProbabilityClass = "probability-low";
	var highProbabilityClass = "probability-high";
	var expiredProbabilityClass = "probability-expired";

	var startOfToday = moment(rawProbability.StartTime).startOf("day");
	var startMoment = moment(rawProbability.StartTime);
	var endMoment = moment(rawProbability.EndTime);
	var intervalTimeSpan = getIntervalTimeSpan(startMoment, endMoment);
	var intervalEndMinutes = getIntervalEndMinutes(startOfToday, startMoment, endMoment);
	var intervalStartMinutes = getIntervalStartMinutes(startOfToday, startMoment);

	if (!shouldGenerateViewModel(intervalStartMinutes, intervalEndMinutes)) return {};

	var visible = isVisible(intervalStartMinutes, intervalEndMinutes);

	function isVisible(intervalStartMinutes, intervalEndMinutes) {
		var visible = false;
		if (probabilityType === constants.absenceProbabilityType) {
			for (var i = 0; i < continousPeriods.length; i++) {
				var continousPeriod = continousPeriods[i];
				if (continousPeriod.startTime <= intervalStartMinutes && intervalEndMinutes <= continousPeriod.endTime) {
					visible = true;
					break;
				}
			}
		} else if (probabilityType === constants.overtimeProbabilityType) {
			visible = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
				intervalEndMinutes <= boundaries.probabilityEndMinutes;
		}
		return visible;
	}

	function generateCssClass() {
		var cssClass = invisibleProbabilityClass;

		if(visible) {
			if (rawProbability.Possibility === constants.probabilityLow)
				cssClass = lowProbabilityClass;
			else if (rawProbability.Possibility === constants.probabilityHigh)
				cssClass = highProbabilityClass;
		}

		if (parent.userNowInMinute() < 0) {
			return invisibleProbabilityClass;
		} else if (parent.userNowInMinute() < intervalEndMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + expiredProbabilityClass;
		}
	}

	function getStyleJson() {
		var styleJson = {};
		var lengthPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * constants.intervalLengthInMinutes * 100;
		var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";
		styleJson[lengthProperty] = lengthPerIntervalInPercentage + "%";
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
		if (!visible || !(parent.userNowInMinute() >= 0 && parent.userNowInMinute() < intervalEndMinutes))
			return "";

		var label = "",
			tooltipTitle = getTooltipsTitle();
		if (rawProbability.Possibility === constants.probabilityLow)
			label = userTexts.low;
		else if (rawProbability.Possibility === constants.probabilityHigh)
			label = userTexts.high;

		return "<div>"
			+ "  <div>" + tooltipTitle + "</div>"
			+ "  <div class='tooltip-wordwrap'>" + label + "</div>"
			+ "  <div class='tooltip-wordwrap'>" + intervalTimeSpan + "</div>"
			+ "</div>";
	}

	function getIntervalStartMinutes(startOfToday, startMoment) {
		return startMoment.diff(startOfToday) / (60 * 1000);
	}

	function getIntervalEndMinutes(startOfToday, startMoment, endMoment) {
		return endMoment.isSame(startMoment, "day")
		? endMoment.diff(startOfToday) / (60 * 1000)
		: constants.totalMinutesOfOneDay - 1;
	}

	function getIntervalTimeSpan(startMoment, endMoment) {
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		var dayDiff = endMoment.diff(startOfToday, "days");
		return startMoment.format(timeFormat) + " - " + endMoment.format(timeFormat) + (dayDiff > 0 ? " +" + dayDiff : "");
	}

	function shouldGenerateViewModel(intervalStartMinutes, intervalEndMinutes) {
		return boundaries.probabilityStartMinutes <= intervalStartMinutes &&
			intervalEndMinutes <= boundaries.probabilityEndMinutes;
	}

	return {
		styleJson: getStyleJson(),
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};
