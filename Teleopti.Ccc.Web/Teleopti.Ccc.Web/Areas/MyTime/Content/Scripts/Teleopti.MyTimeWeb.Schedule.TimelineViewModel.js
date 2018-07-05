Teleopti.MyTimeWeb.Schedule.TimelineViewModel = function(
	rawTimeline,
	scheduleHeight,
	offset,
	useFixedContainerHeight,
	index
) {
	var self = this;
	var fullDayHour = '1.00';
	var hourMinuteSecond = rawTimeline.Time.split(':');
	var hour = hourMinuteSecond[0];
	var minute = hourMinuteSecond[1];

	if (hour.toString() === fullDayHour) {
		self.minutes = Teleopti.MyTimeWeb.Common.Constants.totalMinutesOfOneDay;
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
		else return index * 60 + (offset || 0) + 'px';
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
