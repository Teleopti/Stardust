Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function(rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent, layoutDirection) {
	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityClass = "probability-none";
	var lowProbabilityClass = "probability-low";
	var highProbabilityClass = "probability-high";
	var expiredProbabilityClass = "probability-expired";

	var startOfToday = moment(rawProbability.StartTime).startOf("day");
	var startMoment = moment(rawProbability.StartTime);
	var endMoment = moment(rawProbability.EndTime);
	var intervalStartMinutes = getIntervalStartMinutes(startOfToday, startMoment);
	var intervalEndMinutes = getIntervalEndMinutes(startOfToday, startMoment, endMoment);
	var startDiffInMin = endDiffInMin = 0;

	if (!shouldGenerateViewModel(intervalStartMinutes, intervalEndMinutes)) return {};

	function trimIntervalAccordingSchedulePeriod() {
		if (probabilityType === constants.absenceProbabilityType) {
			for (var i = 0; i < continousPeriods.length; i++) {
				var continousPeriod = continousPeriods[i];
				if ((intervalStartMinutes <= continousPeriods[i].startTimeInMin && intervalEndMinutes >= continousPeriods[i].startTimeInMin)) {
					startDiffInMin = continousPeriods[i].startTimeInMin - intervalStartMinutes;
					intervalStartMinutes = continousPeriods[i].startTimeInMin;
				}

				if ((intervalStartMinutes <= continousPeriods[i].endTimeInMin && intervalEndMinutes >= continousPeriods[i].endTimeInMin)) {
					endDiffInMin = intervalEndMinutes - continousPeriods[i].endTimeInMin;
					intervalEndMinutes = continousPeriods[i].endTimeInMin;
				}
			}
		} else if (probabilityType === constants.overtimeProbabilityType) {
			visible = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
				intervalEndMinutes <= boundaries.probabilityEndMinutes;
		}
	}

	function generateCssClass() {
		trimIntervalAccordingSchedulePeriod();

		if (rawProbability.Possibility === constants.probabilityLow)
			cssClass = lowProbabilityClass;
		else if (rawProbability.Possibility === constants.probabilityHigh)
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
		var intervalStartPositionPercentage = boundaries.lengthPercentagePerMinute * intervalStartMinutes * 100;
		var positionProperty = layoutDirection === constants.horizontalDirectionLayout ? "left" : "top";
		styleJson[positionProperty] = intervalStartPositionPercentage + "%";

		var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";
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
			intervalTimeSpanText = generateIntervalTimeSpanText(startMoment, endMoment);

		if (rawProbability.Possibility === constants.probabilityLow)
			label = userTexts.low;
		else if (rawProbability.Possibility === constants.probabilityHigh)
			label = userTexts.high;

		return "<div>" +
			"  <div>" + tooltipTitle + "</div>" +
			"  <div class='tooltip-wordwrap'>" + label + "</div>" +
			"  <div class='tooltip-wordwrap'>" + intervalTimeSpanText + "</div>" +
			"</div>";
	}

	function getIntervalStartMinutes(startOfToday, startMoment) {
		return startMoment.diff(startOfToday) / (60 * 1000);
	}

	function getIntervalEndMinutes(startOfToday, startMoment, endMoment) {
		return endMoment.isSame(startMoment, "day") ?
			endMoment.diff(startOfToday) / (60 * 1000) :
			constants.totalMinutesOfOneDay - 1;
	}

	function generateIntervalTimeSpanText(startMoment, endMoment) {
		var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
		var dayDiff = endMoment.diff(startOfToday, "days");
		return startMoment.minutes(intervalStartMinutes % 60).format(timeFormat) + " - " + endMoment.minutes(intervalEndMinutes % 60).format(timeFormat) + (dayDiff > 0 ? " +" + dayDiff : "");
	}

	function shouldGenerateViewModel(intervalStartMinutes, intervalEndMinutes) {
		return boundaries.probabilityStartMinutes <= intervalStartMinutes &&
			intervalEndMinutes <= boundaries.probabilityEndMinutes;
	}

	return {
		styleJson: generateStyleJson(),
		cssClass: generateCssClass,
		tooltips: generateTooltips
	};
};