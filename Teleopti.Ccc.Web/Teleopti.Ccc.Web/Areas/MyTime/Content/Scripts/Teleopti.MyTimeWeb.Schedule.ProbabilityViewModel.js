Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbabilityCellData, probabilityType, boundaries, userTexts, parent, layoutDirection, hideProbabilityEarlierThanNow) {
	var startPositionVal,
		intervalLengthVal,
		constants = Teleopti.MyTimeWeb.Common.Constants;

	var probabilityLevel = {
		low: 0,
		high: 1
	};

	var startOfToday = moment(rawProbabilityCellData.startTimeMoment).startOf("day");

	function generateCssClass() {
		var cssClass = "";
		if (rawProbabilityCellData.possibility === probabilityLevel.low)
			cssClass = constants.probabilityClass.lowProbabilityClass;
		else if (rawProbabilityCellData.possibility === probabilityLevel.high)
			cssClass = constants.probabilityClass.highProbabilityClass;

		if (parent.userNowInMinute() < rawProbabilityCellData.endTimeInMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + constants.probabilityClass.expiredProbabilityClass;
		}
	}

	function generateStyleJson() {
		var styleJson = {};
		var startPositionProperty = layoutDirection === constants.layoutDirection.horizontal ? "left" : "top";
		var lengthProperty = layoutDirection === constants.layoutDirection.horizontal ? "width" : "height";

		startPositionVal = (Math.ceil(boundaries.lengthPercentagePerMinute *
				(rawProbabilityCellData.startTimeInMinutes - boundaries.timelineStartMinutes) *
				10000) /
			100).toFixed(2);

		var percentagePerMinute = Math.ceil(boundaries.lengthPercentagePerMinute * 10000) / 10000;
		intervalLengthVal = (Math.ceil(percentagePerMinute *
				(rawProbabilityCellData.endTimeInMinutes - rawProbabilityCellData.startTimeInMinutes) *
				10000) /
			100).toFixed(2);

		styleJson[startPositionProperty] = startPositionVal + "%";
		styleJson[lengthProperty] = intervalLengthVal + "%";

		return styleJson;
	}

	function getTooltipsTitle() {
		var result = "";
		if (probabilityType === constants.probabilityType.absence) {
			result = userTexts.ProbabilityToGetAbsenceColon;
		} else if (probabilityType === constants.probabilityType.overtime) {
			result = userTexts.ProbabilityToGetOvertimeColon;
		}
		return result;
	}

	function generateTooltips() {
		if (!hideProbabilityEarlierThanNow || parent.userNowInMinute() <= 0 || (parent.userNowInMinute() > 0 && parent.userNowInMinute() < rawProbabilityCellData.endTimeInMinutes)) {
			var label = "",
				tooltipTitle = getTooltipsTitle(),
				intervalTimeSpanText = generateIntervalTimeSpanText(rawProbabilityCellData.startTimeMoment, rawProbabilityCellData.endTimeMoment);

			if (rawProbabilityCellData.possibility === probabilityLevel.low)
				label = userTexts.Low;
			else if (rawProbabilityCellData.possibility === probabilityLevel.high)
				label = userTexts.High;

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
		startPosition: startPositionVal,
		intervalLength: intervalLengthVal,
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};