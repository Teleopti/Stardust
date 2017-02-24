Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel = function (rawProbability, probabilityType, boundaries,
	continousPeriods, userTexts, parent) {

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

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

	var heightPerIntervalInPx = boundaries.heightPercentagePerMinute * constants.intervalLengthInMinutes *
		constants.scheduleHeight;
	return {
		startMinutes: intervalStartMinutes,
		endInMinutes: intervalEndMinutes,
		actualClass: cssClass,
		actualTooltips: tooltips,
		styleJson: { "height": heightPerIntervalInPx + "px" },
		cssClass: function () {
			return (parent.userNowInMinute() >= 0 && parent.userNowInMinute() < this.endInMinutes)
				? this.actualClass
				: "probability-none";
		},
		tooltips: function () {
			return (parent.userNowInMinute() >= 0 && parent.userNowInMinute() < intervalEndMinutes)
				? tooltips
				: "";
		}
	};
};