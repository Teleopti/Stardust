Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbabilityCellData, parent, boundaries, options) {
	var startPositionVal,
		intervalLengthVal,
		constants = Teleopti.MyTimeWeb.Common.Constants;

	var startOfToday = moment(rawProbabilityCellData.startTimeMoment).startOf("day");

	function generateCssClass() {
		var cssClass = "";
		if (rawProbabilityCellData.possibility === constants.probabilityLevel.low)
			cssClass = constants.probabilityClass.lowProbabilityClass;
		else if (rawProbabilityCellData.possibility === constants.probabilityLevel.high)
			cssClass = constants.probabilityClass.highProbabilityClass;

		if (parent.userNowInMinute < rawProbabilityCellData.endTimeInMinutes) {
			return cssClass;
		} else {
			return cssClass + " " + constants.probabilityClass.expiredProbabilityClass;
		}
	}

	function generateStyleJson() {
		var styleJson = {};
		var startPositionProperty = options.layoutDirection === constants.layoutDirection.horizontal ? "left" : "top";
		var lengthProperty = options.layoutDirection === constants.layoutDirection.horizontal ? "width" : "height";
		var percentagePerMinute = boundaries.lengthPercentagePerMinute;

		var probabilityStartInMinutes = rawProbabilityCellData.startTimeInMinutes, 
			probabilityEndInMinutes = rawProbabilityCellData.endTimeInMinutes;

		if(options.daylightSavingTimeAdjustment && options.daylightSavingTimeAdjustment.EnteringDST) {
			if(probabilityStartInMinutes >= options.daylightSavingTimeAdjustment.LocalDSTStartTimeInMinutes) {
				probabilityStartInMinutes -= options.daylightSavingTimeAdjustment.AdjustmentOffsetInMinutes;
			}

			if(probabilityEndInMinutes >= options.daylightSavingTimeAdjustment.LocalDSTStartTimeInMinutes) {
				probabilityEndInMinutes -= options.daylightSavingTimeAdjustment.AdjustmentOffsetInMinutes;
			}
		}

		startPositionVal = (percentagePerMinute * (probabilityStartInMinutes - boundaries.timelineStartMinutes + 0.1) * 100).toFixed(2);
		
		intervalLengthVal = (percentagePerMinute * (probabilityEndInMinutes - probabilityStartInMinutes + 0.1) * 100).toFixed(2);

		styleJson[startPositionProperty] = startPositionVal + "%";
		styleJson[lengthProperty] = intervalLengthVal + "%";

		return styleJson;
	}

	function getTooltipsTitle() {
		var result = "";
		if (options.probabilityType === constants.probabilityType.absence) {
			result = options.userTexts.ProbabilityToGetAbsenceColon;
		} else if (options.probabilityType === constants.probabilityType.overtime) {
			result = options.userTexts.ProbabilityToGetOvertimeColon;
		}
		return result;
	}

	function generateTooltips() {
		if (!options.hideProbabilityEarlierThanNow || parent.userNowInMinute <= 0 || (parent.userNowInMinute > 0 && parent.userNowInMinute < rawProbabilityCellData.endTimeInMinutes)) {
			var label = "",
				tooltipTitle = getTooltipsTitle(),
				intervalTimeSpanText = generateIntervalTimeSpanText(rawProbabilityCellData.startTimeMoment, rawProbabilityCellData.endTimeMoment);

			if (rawProbabilityCellData.possibility === constants.probabilityLevel.low)
				label = options.userTexts.Low;
			else if (rawProbabilityCellData.possibility === constants.probabilityLevel.high)
				label = options.userTexts.High;

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