Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary = function(
	dayViewModel,
	timelines,
	probabilityType,
	probabilities,
	daylightSavingTimeAdjustment
) {
	var constants = Teleopti.MyTimeWeb.Common.Constants;

	var shiftStartMinutes = -1;
	var shiftEndMinutes = constants.totalMinutesOfOneDay + 1;

	var probabilityStartMinutes;
	var probabilityEndMinutes;

	var timelineStartMinutes = timelines[0].minutes;
	var timelineEndMinutes = timelines[timelines.length - 1].minutes;

	if (timelineEndMinutes === constants.totalMinutesOfOneDay - 1) {
		timelineEndMinutes = constants.totalMinutesOfOneDay;
	}

	// If timeline is not start or end at 00:00, there will exist an extra 15 minutes at start or end
	// Need handle with this scenario.
	var timelineStartMinutesForBoundary =
		timelineStartMinutes > 0 ? timelineStartMinutes + constants.timelineMarginInMinutes : 0;
	var timelineEndMinutesForBoundary =
		timelineEndMinutes < constants.totalMinutesOfOneDay
			? timelineEndMinutes - constants.timelineMarginInMinutes
			: timelineEndMinutes;

	var totalLength = timelineEndMinutes - timelineStartMinutes;
	if (daylightSavingTimeAdjustment && daylightSavingTimeAdjustment.EnteringDST) {
		totalLength = totalLength - daylightSavingTimeAdjustment.AdjustmentOffsetInMinutes;
	}

	var lengthPercentagePerMinute = 1 / totalLength;

	var momentDate = Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(
		dayViewModel.fixedDate && dayViewModel.fixedDate._isAMomentObject
			? dayViewModel.fixedDate.format('YYYY-MM-DD')
			: dayViewModel.fixedDate
	).startOf('day');

	if (dayViewModel.periods.length > 0) {
		var firstPeriod = dayViewModel.periods[0];
		var lastPeriod = dayViewModel.periods[dayViewModel.periods.length - 1];

		shiftStartMinutes =
			Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(firstPeriod.StartTime).diff(momentDate) / (60 * 1000);
		if (shiftStartMinutes < 0) {
			shiftStartMinutes = 0;
		}

		shiftEndMinutes =
			Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(lastPeriod.EndTime).diff(momentDate) / (60 * 1000);
		if (shiftEndMinutes > constants.totalMinutesOfOneDay) {
			shiftEndMinutes = constants.totalMinutesOfOneDay;
		}
	}

	var rawProbabilityStartMinutes = -1;
	var rawProbabilityEndMinutes = constants.totalMinutesOfOneDay + 1;
	if (probabilities.length > 0) {
		var firstProbabilityStartTime = Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(
			probabilities[0].StartTime
		);
		var firstProbabilityStartMinute = firstProbabilityStartTime.diff(momentDate) / (60 * 1000);
		if (firstProbabilityStartMinute < 0) {
			firstProbabilityStartMinute = 0;
		}

		if (firstProbabilityStartMinute > rawProbabilityStartMinutes) {
			rawProbabilityStartMinutes = firstProbabilityStartMinute;
		}

		var lastProbabilityEndTime = Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(
			probabilities[probabilities.length - 1].EndTime
		);
		var lastProbabilityEndMinute = lastProbabilityEndTime.diff(momentDate) / (60 * 1000);
		if (lastProbabilityEndMinute > constants.totalMinutesOfOneDay) {
			lastProbabilityEndMinute = constants.totalMinutesOfOneDay;
		}

		if (rawProbabilityEndMinutes > lastProbabilityEndMinute) {
			rawProbabilityEndMinutes = lastProbabilityEndMinute;
		}
	}

	var openPeriodStartMinutes = -1;
	var openPeriodEndMinutes = constants.totalMinutesOfOneDay + 1;
	if (probabilityType === constants.probabilityType.overtime && dayViewModel.openHourPeriod != undefined) {
		openPeriodStartMinutes = moment.duration(dayViewModel.openHourPeriod.StartTime).asMinutes();
		openPeriodEndMinutes = moment.duration(dayViewModel.openHourPeriod.EndTime).asMinutes();
	}

	var startTimeCandidates = [rawProbabilityStartMinutes, timelineStartMinutesForBoundary];
	var endTimeCandidates = [rawProbabilityEndMinutes, timelineEndMinutesForBoundary];

	if (probabilityType === constants.probabilityType.absence) {
		startTimeCandidates.push(shiftStartMinutes);
		endTimeCandidates.push(shiftEndMinutes);
	} else if (probabilityType === constants.probabilityType.overtime) {
		startTimeCandidates.push(openPeriodStartMinutes);
		endTimeCandidates.push(openPeriodEndMinutes);
	}

	probabilityStartMinutes = Math.max.apply(null, startTimeCandidates);
	probabilityEndMinutes = Math.min.apply(null, endTimeCandidates);

	return {
		timelineStartMinutes: timelineStartMinutes,
		timelineEndMinutes: timelineEndMinutes,
		lengthPercentagePerMinute: lengthPercentagePerMinute,
		probabilityStartMinutes: probabilityStartMinutes,
		probabilityEndMinutes: probabilityEndMinutes
	};
};
