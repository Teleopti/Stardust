Teleopti.MyTimeWeb.Schedule.TimelineViewModel = function(rawTimeline, scheduleHeight, offset) {
	var self = this;
	var fullDayHour = "1.00";
	var hourMinuteSecond = rawTimeline.Time.split(':');
	var hour = hourMinuteSecond[0];

	self.positionPercentage = ko.observable(rawTimeline.PositionPercentage);	

	if (hour.toString() === fullDayHour) {
		self.minutes = Teleopti.MyTimeWeb.Common.Constants.totalMinutesOfOneDay;
	} else {
		self.minutes = hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1]);
	}

	var timeFromMinutes = moment()
		.startOf('day')
		.add('minutes', self.minutes);

	self.timeText = rawTimeline.TimeLineDisplay;

	self.topPosition = ko.computed(function() {
		return Math.round(scheduleHeight * self.positionPercentage()) + (offset || 0) + 'px';
	});

	self.isHour = ko.computed(function() {
		return timeFromMinutes.minute() === 0;
	});
};