Teleopti.MyTimeWeb.Schedule.TimelineViewModel = function(
	rawTimeline,
	scheduleHeight,
	offset,
	useFixedContainerHeight,
	index
) {
	var self = this,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		FULL_DAY_HOUR_STR = constants.fullDayHourStr,
		TOTAL_MINUTES_OF_ONE_DAY = constants.totalMinutesOfOneDay,
		PIXEL_OF_ONE_HOUR = constants.pixelOfOneHourInTeamSchedule,
		hourMinuteSecond = rawTimeline.Time.split(':'),
		hour = hourMinuteSecond[0],
		minute = hourMinuteSecond[1];

	if (hour.toString() === FULL_DAY_HOUR_STR) {
		self.minutes = TOTAL_MINUTES_OF_ONE_DAY;
	} else {
		if (hour.indexOf('.') > -1) {
			hour = 24 * parseInt(hour.split('.')[0]) + parseInt(hour.split('.')[1]);
		}
		self.minutes = hour * 60 + parseInt(minute);
	}

	self.positionPercentage = ko.observable(rawTimeline.PositionPercentage);
	self.timeText = rawTimeline.TimeLineDisplay;
	self.time = rawTimeline.Time;

	self.topPosition = ko.computed(function() {
		if (useFixedContainerHeight)
			return Math.round(scheduleHeight * self.positionPercentage()) + (offset || 0) + 'px';
		else return index * PIXEL_OF_ONE_HOUR + (offset || 0) + 'px';
	});

	self.isHour = ko.computed(function() {
		return (
			moment()
				.startOf('day')
				.add('minutes', self.minutes)
				.minute() === 0
		);
	});
};
