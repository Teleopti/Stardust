Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary = function(
	dayViewModel,
	timelines,
	probabilityType,
	probabilities,
	daylightSavingTimeAdjustment,
	allowOvernight
) {
	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var momentUtc = Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone;

	var shiftStartMinutes = -1;
	var shiftEndMinutes = constants.totalMinutesOfOneDay + 1;

	var probabilityStartMinutes = 0;
	var probabilityEndMinutes = 0;

	var timelineStartMinutes = timelines[0].minutes;
	var timelineEndMinutes = timelines[timelines.length - 1].minutes;

	if (!allowOvernight && timelineEndMinutes === constants.totalMinutesOfOneDay - 1) {
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

	var momentDate = momentUtc(
		dayViewModel.fixedDate && dayViewModel.fixedDate._isAMomentObject
			? dayViewModel.fixedDate.format('YYYY-MM-DD')
			: dayViewModel.fixedDate
	).startOf('day');

	if (dayViewModel.periods.length > 0) {
		var firstPeriod = dayViewModel.periods[0];
		var lastPeriod = dayViewModel.periods[dayViewModel.periods.length - 1];

		shiftStartMinutes = getOffsetInMinute(firstPeriod.StartTime);
		if (shiftStartMinutes < 0) {
			shiftStartMinutes = 0;
		}

		shiftEndMinutes = momentUtc(lastPeriod.EndTime).diff(momentDate) / (60 * 1000);
		if (!allowOvernight && shiftEndMinutes > constants.totalMinutesOfOneDay) {
			shiftEndMinutes = constants.totalMinutesOfOneDay;
		}
	}

	var rawProbabilityStartMinutes = -1;
	var rawProbabilityEndMinutes =
		allowOvernight && probabilities[probabilities.length - 1]
			? getOffsetInMinute(probabilities[probabilities.length - 1].EndTime)
			: constants.totalMinutesOfOneDay + 1;

	if (probabilities.length > 0) {
		var firstProbabilityStartMinute = getOffsetInMinute(probabilities[0].StartTime);
		if (firstProbabilityStartMinute < 0) {
			firstProbabilityStartMinute = 0;
		}

		if (firstProbabilityStartMinute > rawProbabilityStartMinutes) {
			rawProbabilityStartMinutes = firstProbabilityStartMinute;
		}

		var lastProbabilityEndTime = momentUtc(probabilities[probabilities.length - 1].EndTime);
		var lastProbabilityEndMinute = lastProbabilityEndTime.diff(momentDate) / (60 * 1000);
		if (!allowOvernight && lastProbabilityEndMinute > constants.totalMinutesOfOneDay) {
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

		if (allowOvernight && openPeriodStartMinutes == 0 && openPeriodEndMinutes === constants.totalMinutesOfOneDay) {
			openPeriodEndMinutes = rawProbabilityEndMinutes;
		}
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

	function getOffsetInMinute(time) {
		return momentUtc(time).diff(momentDate) / (60 * 1000);
	}

	return {
		timelineStartMinutes: timelineStartMinutes,
		timelineEndMinutes: timelineEndMinutes,
		lengthPercentagePerMinute: lengthPercentagePerMinute,
		probabilityStartMinutes: probabilityStartMinutes,
		probabilityEndMinutes: probabilityEndMinutes
	};
};
