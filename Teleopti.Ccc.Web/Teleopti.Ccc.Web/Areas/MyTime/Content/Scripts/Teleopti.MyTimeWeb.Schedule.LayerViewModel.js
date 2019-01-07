Teleopti.MyTimeWeb.Schedule.LayerViewModel = function(
	layer,
	parent,
	layersOnMobile,
	offset,
	useFixedContainerHeight,
	timelineStart,
	selectedDate,
	cleanOvernightNumber,
	isMySchedule
) {
	var self = this;
	var common = Teleopti.MyTimeWeb.Common;
	var userTexts = common.GetUserTexts();
	var constants = common.Constants;
	var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();

	self.title = layer.Title;
	self.hasMeeting = layer.Meeting !== null;
	self.timeSpan = getTimeSpan(layer.TimeSpan, cleanOvernightNumber);
	self.tooltipText = buildTooltipText(self.title, self.timeSpan, self.hasMeeting,isMySchedule);
	self.backgroundColor = Teleopti.MyTimeWeb.Common.ConvertColorToRGB(layer.Color);
	self.textColor = Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.backgroundColor);
	self.startPositionPercentage = layer.StartPositionPercentage;
	self.endPositionPercentage = layer.EndPositionPercentage;
	self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
	self.isOvertimeAvailability = layer.IsOvertimeAvailability;
	self.isOvertime = layer.IsOvertime;

	self.top = getTopValue(
		self.startPositionPercentage,
		scheduleHeight,
		useFixedContainerHeight,
		timelineStart,
		layer.StartTime,
		offset
	);

	self.height = getHeight(
		self.endPositionPercentage,
		scheduleHeight,
		useFixedContainerHeight,
		timelineStart,
		layer.EndTime,
		offset,
		self.top
	);

	self.topPx = self.top + 'px';
	self.heightPx = self.height + 'px';
	self.heightDouble = scheduleHeight * (self.endPositionPercentage - self.startPositionPercentage);
	self.widthPx = getWidth(layer.IsOvertimeAvailability);

	self.overTimeLighterBackgroundStyle = getOverTimeLighterBackgroundStyle(self.backgroundColor);
	self.overTimeDarkerBackgroundStyle = !self.overTimeLighterBackgroundStyle;

	self.showTitle = self.heightDouble > constants.pixelToDisplayTitle;
	self.showDetail = self.heightDouble > constants.pixelToDisplayAll;

	self.styleJson = {
		top: self.topPx,
		width: self.widthPx,
		height: self.heightPx,
		color: self.textColor,
		'background-size': self.isOvertime ? '11px 11px' : 'initial',
		'background-color': self.backgroundColor
	};

	function getTimeSpan(originalTimespan, cleanOvernightNumber) {
		// Remove extra space for extreme long timespan (For example: "10:00 PM - 12:00 AM +1")
		var realTimespan =
			originalTimespan.length >= 22
				? originalTimespan.replace(' - ', '-').replace(' +1', '+1')
				: originalTimespan;

		if (cleanOvernightNumber) {
			realTimespan = realTimespan.replace(' -1', '').replace(' +1', '');
			realTimespan = realTimespan.replace('-1', '').replace('+1', '');
		}
		return realTimespan;
	}

	function buildTooltipText(title, timeSpan, hasMeeting, isMySchedule) {
		var tooltipContent = '<div>{0}</div>'.format(title);
		if (!hasMeeting) {
			return tooltipContent + timeSpan;
		}

		var meetingDescription = layer.Meeting.Description;
		if (meetingDescription && meetingDescription.length > 200) {
			meetingDescription = meetingDescription.substring(0, 200) + '...';
		}

		var timeSpanText = "<div>{0}</div>".format(timeSpan);

		if (!isMySchedule)
			return tooltipContent + timeSpanText;

		var meetingText = (
			"<div class='meeting-detail' style='text-align: left'>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{0}</i> {1}</div>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{2}</i> {3}</div>" +
			"<div class='tooltip-wordwrap' style='white-space: normal'><i>{4}</i> {5}</div>" +
			'</div>'
		).format(
			userTexts.SubjectColon,
			$('<div/>')
				.text(layer.Meeting.Title)
				.html(),
			userTexts.LocationColon,
			$('<div/>')
				.text(layer.Meeting.Location)
				.html(),
			userTexts.DescriptionColon,
			$('<div/>')
				.text(meetingDescription)
				.html()
		);

		return tooltipContent + timeSpanText + meetingText;
	}

	function getTopValue(startPos, scheduleHeight, useFixedContainerHeight, timelineStart, layerStartTime, offset) {
		if (useFixedContainerHeight) {
			return Math.round(scheduleHeight * startPos) + (offset || 0);
		} else if (timelineStart) {
			var milisecondsToTimeLineStart = 0;
			if (timelineStart.indexOf('T') > -1) {
				milisecondsToTimeLineStart = moment(layerStartTime) - moment(timelineStart);
			} else {
				var startStr = timelineStart.length == 10 ? timelineStart.slice(-8, -3) : timelineStart.slice(0, 5);

				milisecondsToTimeLineStart = moment.duration(layerStartTime.split('T')[1]) - moment.duration(startStr);
				if (!moment(layerStartTime.split('T')[0]).isSame(moment(selectedDate), 'day')) {
					milisecondsToTimeLineStart += 24 * 60 * 60 * 1000;
				}
			}

			var top =
				parseInt(milisecondsToTimeLineStart / (60 * 1000)) +
				(milisecondsToTimeLineStart % (60 * 1000)) +
				(offset || 0);
			return top;
		}
	}

	function getHeight(endPos, scheduleHeight, useFixedContainerHeight, timelineStart, layerEndTime, offset, top) {
		var bottom = 0;

		if (useFixedContainerHeight) {
			bottom = Math.round(scheduleHeight * endPos) + 1 + (offset || 0);
		} else if (timelineStart) {
			var bottomDurationInMiliseconds = 0;
			if (timelineStart.indexOf('T') > -1) {
				bottomDurationInMiliseconds = moment(layerEndTime) - moment(timelineStart);
			} else {
				var startStr = timelineStart.length == 10 ? timelineStart.slice(-8, -3) : timelineStart.slice(0, 5);

				bottomDurationInMiliseconds = moment.duration(layerEndTime.split('T')[1]) - moment.duration(startStr);
				if (!moment(layerEndTime.split('T')[0]).isSame(moment(selectedDate), 'day')) {
					bottomDurationInMiliseconds += 24 * 60 * 60 * 1000;
				}
			}

			bottom =
				parseInt(bottomDurationInMiliseconds / (60 * 1000)) +
				(bottomDurationInMiliseconds % (60 * 1000)) +
				(offset || 0);
		}

		return bottom > top ? bottom - top : 0;
	}

	function getWidth(isOvertimeAvailability) {
		var width;
		if (isOvertimeAvailability) {
			width = 20 + '%';
		} else {
			width = 'calc(100% - 2px)';
		}
		return width;
	}

	function getOverTimeLighterBackgroundStyle(bgColor) {
		function getLumi(cstring) {
			var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
			if (!matched) return null;
			return (
				(299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) /
				1000
			);
		}

		var lightColor = '#00ffff';
		var darkColor = '#795548';
		var backgroundColor = common.RGBTohex(bgColor);
		var useLighterStyle =
			Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) >
			Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

		return useLighterStyle;
	}
};
