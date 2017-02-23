Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary = function (scheduleDay, timelines, probabilityType,
	probabilities, intradayOpenPeriod) {
	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

	var shiftStartMinutes = 0;
	var shiftEndMinutes = constants.totalMinutesOfOneDay;

	var probabilityStartMinutes;
	var probabilityEndMinutes;
	var probabilityStartPosition;
	var probabilityEndPosition;

	var timelineStartMinutes = timelines[0].minutes;
	var timelineEndMinutes = timelines[timelines.length - 1].minutes;
	if (timelineEndMinutes === constants.totalMinutesOfOneDay - 1) {
		timelineEndMinutes = constants.totalMinutesOfOneDay;
	}

	// If timeline is not start or end at 00:00, there will exist an extra 15 minutes at start or end
	// Need handle with this scenario.
	var timelineStartMinutesForBoundary = timelineStartMinutes > 0
		? timelineStartMinutes + constants.intervalLengthInMinutes
		: 0;
	var timelineEndMinutesForBoundary = timelineEndMinutes < constants.totalMinutesOfOneDay
		? timelineEndMinutes - constants.intervalLengthInMinutes
		: timelineEndMinutes;

	var heightPercentagePerMinute = 1 / (timelineEndMinutes - timelineStartMinutes);

	var momentDate = moment(scheduleDay.FixedDate);

	function convertTimePointToMinutes(time) {
		var baseDate = "2017-01-01 ";
		return (moment(baseDate + time).diff(moment(baseDate)) / (60 * 1000));
	}

	shiftStartMinutes = -1;
	shiftEndMinutes = constants.totalMinutesOfOneDay + 1;
	if (scheduleDay.IsFullDayAbsence || scheduleDay.IsDayOff) {
		shiftStartMinutes = timelineStartMinutesForBoundary;
		shiftEndMinutes = timelineEndMinutesForBoundary;
	}
	if (scheduleDay.Periods.length > 0) {
		var firstPeriod = scheduleDay.Periods[0];
		var lastPeriod = scheduleDay.Periods[scheduleDay.Periods.length - 1];

		shiftStartMinutes = moment(firstPeriod.StartTime).diff(momentDate) / (60 * 1000);
		if (shiftStartMinutes < 0) {
			shiftStartMinutes = 0;
		}

		shiftEndMinutes = moment(lastPeriod.EndTime).diff(momentDate) / (60 * 1000);
		if (shiftEndMinutes > constants.totalMinutesOfOneDay) {
			shiftEndMinutes = constants.totalMinutesOfOneDay;
		}
	}

	var rawProbabilityStartMinutes = -1;
	var rawProbabilityEndMinutes = constants.totalMinutesOfOneDay + 1;
	if (probabilities.length > 0) {
		var firstProbabilityStartTime = moment(probabilities[0].StartTime);
		var firstProbabilityStartMinute = (firstProbabilityStartTime.diff(momentDate)) / (60 * 1000);
		if (firstProbabilityStartMinute < 0) {
			firstProbabilityStartMinute = 0;
		}

		if (firstProbabilityStartMinute > rawProbabilityStartMinutes) {
			rawProbabilityStartMinutes = firstProbabilityStartMinute;
		}

		var lastProbabilityEndTime = moment(probabilities[probabilities.length - 1].EndTime);
		var lastProbabilityEndMinute = (lastProbabilityEndTime.diff(momentDate)) / (60 * 1000);
		if (lastProbabilityEndMinute > constants.totalMinutesOfOneDay) {
			lastProbabilityEndMinute = constants.totalMinutesOfOneDay;
		}

		if (rawProbabilityEndMinutes > lastProbabilityEndMinute) {
			rawProbabilityEndMinutes = lastProbabilityEndMinute;
		}
	}

	var openPeriodStartMinutes = -1;
	var openPeriodEndMinutes = constants.totalMinutesOfOneDay + 1;
	if (probabilityType === constants.overtimeProbabilityType &&
		intradayOpenPeriod != undefined && intradayOpenPeriod != null) {
		openPeriodStartMinutes = convertTimePointToMinutes(intradayOpenPeriod.startTime);
		openPeriodEndMinutes = convertTimePointToMinutes(intradayOpenPeriod.endTime);
	}

	var startTimeCandidates = [
		rawProbabilityStartMinutes, timelineStartMinutesForBoundary
	];
	var endTimeCandidates = [
		rawProbabilityEndMinutes, timelineEndMinutesForBoundary
	];

	if (probabilityType === constants.absenceProbabilityType) {
		startTimeCandidates.push(shiftStartMinutes);
		endTimeCandidates.push(shiftEndMinutes);
	} else if (probabilityType === constants.overtimeProbabilityType) {
		startTimeCandidates.push(openPeriodStartMinutes);
		endTimeCandidates.push(openPeriodEndMinutes);
	}

	probabilityStartMinutes = Math.max.apply(null, startTimeCandidates);
	probabilityEndMinutes = Math.min.apply(null, endTimeCandidates);

	probabilityStartPosition = (probabilityStartMinutes - timelineStartMinutes) * heightPercentagePerMinute;
	probabilityEndPosition = (probabilityEndMinutes - timelineStartMinutes) * heightPercentagePerMinute;

	return {
		heightPercentagePerMinute: heightPercentagePerMinute,
		shiftStartMinutes: shiftStartMinutes,
		shiftEndMinutes: shiftEndMinutes,
		probabilityStartMinutes: probabilityStartMinutes,
		probabilityEndMinutes: probabilityEndMinutes,
		probabilityStartPosition: probabilityStartPosition,
		probabilityEndPosition: probabilityEndPosition
	};
};