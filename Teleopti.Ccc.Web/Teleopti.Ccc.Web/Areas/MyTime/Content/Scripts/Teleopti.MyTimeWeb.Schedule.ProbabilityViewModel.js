Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent, layoutDirection) {

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;
	var invisibleProbabilityClass = "probability-none";
	var expiredProbabilityClass = "probability-expired";

	var probabilityNames = ["low", "high"];
	var probabilityLabels = [userTexts.low, userTexts.high];

	var tooltipsTitle = "";
	if (probabilityType === constants.absenceProbabilityType) {
		tooltipsTitle = userTexts.probabilityForAbsence;
	} else if (probabilityType === constants.overtimeProbabilityType) {
		tooltipsTitle = userTexts.probabilityForOvertime;
	}

	var startOfToday = moment(rawProbability.StartTime).startOf("day");
	var startMoment = moment(rawProbability.StartTime);
	var endMoment = moment(rawProbability.EndTime);

	var intervalStartMinutes = startMoment.diff(startOfToday) / (60 * 1000);
	var intervalEndMinutes = endMoment.isSame(startMoment, "day")
		? endMoment.diff(startOfToday) / (60 * 1000)
		: constants.totalMinutesOfOneDay - 1;

	var shouldGenerateViewModel = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
		intervalEndMinutes <= boundaries.probabilityEndMinutes;
	if (!shouldGenerateViewModel) return {};

	var visible = false;
	if (probabilityType === constants.absenceProbabilityType) {
		for (var m = 0; m < continousPeriods.length; m++) {
			var continousPeriod = continousPeriods[m];
			if (continousPeriod.startTime <= intervalStartMinutes && intervalEndMinutes <= continousPeriod.endTime) {
				visible = true;
				break;
			}
		}
	} else if (probabilityType === constants.overtimeProbabilityType) {
		visible = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
			intervalEndMinutes <= boundaries.probabilityEndMinutes;;
	}

	var index = rawProbability.Possibility;
	var timeFormat = Teleopti.MyTimeWeb.Common.TimeFormat;
	var dayDiff = endMoment.diff(startOfToday, "days");
	var intervalTimeSpan = startMoment.format(timeFormat) + " - " + endMoment.format(timeFormat)
		+ (dayDiff > 0 ? " +" + dayDiff : "");

	var cssClass = visible ? "probability-" + probabilityNames[index] : invisibleProbabilityClass;
	var tooltips = visible
		? "<div style='text-align: center'>"
		+ "  <div>" + tooltipsTitle + "</div>"
		+ "  <div class='tooltip-wordwrap' style='font-weight: bold'>" + probabilityLabels[index] + "</div>"
		+ "  <div class='tooltip-wordwrap' style='overflow: hidden'>" + intervalTimeSpan + "</div>"
		+ "</div>"
		: "";

	var styleJson = {};
	var lengthPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * constants.intervalLengthInMinutes * 100;
	var lengthProperty = layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";
	styleJson[lengthProperty] = lengthPerIntervalInPercentage + "%";

	return {
		startMinutes: intervalStartMinutes,
		endInMinutes: intervalEndMinutes,
		actualClass: cssClass,
		actualTooltips: tooltips,
		styleJson: styleJson,
		cssClass: function () {
			if (parent.userNowInMinute() < 0) {
				return invisibleProbabilityClass;
			} else if (parent.userNowInMinute() < this.endInMinutes) {
				return this.actualClass;
			} else {
				return this.actualClass + " " + expiredProbabilityClass;
			}
		},
		tooltips: function () {
			return (parent.userNowInMinute() >= 0 && parent.userNowInMinute() < intervalEndMinutes)
				? tooltips
				: "";
		}
	};
};