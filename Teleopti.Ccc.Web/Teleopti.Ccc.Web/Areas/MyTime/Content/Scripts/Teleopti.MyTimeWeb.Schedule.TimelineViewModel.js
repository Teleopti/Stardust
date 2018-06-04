Teleopti.MyTimeWeb.Schedule.TimelineViewModel = function(rawTimeline, scheduleHeight, offset) {
	var self = this;
	var fullDayHour = "1.00";
	var hourMinuteSecond = rawTimeline.Time.split(':');
	var hour = hourMinuteSecond[0];
	var minute = hourMinuteSecond[1];

	self.positionPercentage = ko.observable(rawTimeline.PositionPercentage);	

	if (hour.toString() === fullDayHour) {
		self.minutes = Teleopti.MyTimeWeb.Common.Constants.totalMinutesOfOneDay;
	} else {
		if (hour.indexOf('.') > -1){
			hour = hour.split('.')[1];
		}
		self.minutes = hour * 60 + parseInt(minute);
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