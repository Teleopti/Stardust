Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbabilityCellData, probabilityType, boundaries, userTexts, parent, layoutDirection, hideProbabilityEarlierThanNow) {

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var lowProbabilityClass = "probability-low";
	var highProbabilityClass = "probability-high";
	var expiredProbabilityClass = "probability-expired";

	var startOfToday = moment(rawProbabilityCellData.startTimeMoment).startOf("day");

	function generateCssClass() {
		var cssClass = "";
		if (rawProbabilityCellData.possibility === constants.probabilityLow)
			cssClass = lowProbabilityClass;
		else if (rawProbabilityCellData.possibility === constants.probabilityHigh)
			cssClass = highProbabilityClass;

		if (parent.userNowInMinute() < rawProbabilityCellData.endTimeInMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + expiredProbabilityClass;
		}
	}

	function generateStyleJson() {
		var styleJson = {};
		var startPositionProperty = layoutDirection === constants.horizontalDirectionLayout ? "left" : "top";
		var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";

		styleJson[startPositionProperty] = boundaries.lengthPercentagePerMinute * (rawProbabilityCellData.startTimeInMinutes - boundaries.timelineStartMinutes) * 100 + "%";
		styleJson[lengthProperty] = boundaries.lengthPercentagePerMinute * (rawProbabilityCellData.endTimeInMinutes - rawProbabilityCellData.startTimeInMinutes) * 100 + "%";

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
		if (!hideProbabilityEarlierThanNow || parent.userNowInMinute() <= 0 || (parent.userNowInMinute() > 0 && parent.userNowInMinute() < rawProbabilityCellData.endTimeInMinutes)) {
			var label = "",
				tooltipTitle = getTooltipsTitle(),
				intervalTimeSpanText = generateIntervalTimeSpanText(rawProbabilityCellData.startTimeMoment, rawProbabilityCellData.endTimeMoment);

			if (rawProbabilityCellData.possibility === constants.probabilityLow)
				label = userTexts.low;
			else if (rawProbabilityCellData.possibility === constants.probabilityHigh)
				label = userTexts.high;

			return "<div>" +
				"  <div>" + tooltipTitle + "</div>" +
				"  <div class='tooltip-wordwrap'>" + label + "</div>" +
				"  <div class='tooltip-wordwrap'>" + intervalTimeSpanText + "</div>" +
				"</div>";
		} else {
			return "";
		}
	}

	function generateIntervalTimeSpanText(startMoment, endMoment) {
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		var dayDiff = endMoment.diff(startOfToday, "days");
		return startMoment.format(timeFormat) + " - " + endMoment.format(timeFormat) + (dayDiff > 0 ? " +" + dayDiff : "");
	}

	return {
		styleJson: generateStyleJson(),
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};