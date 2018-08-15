﻿Teleopti.MyTimeWeb.Schedule.LayerViewModel = function(
	layer,
	parent,
	layersOnMobile,
	offset,
	useFixedContainerHeight,
	timelineStart,
	selectedDate,
	cleanOvernightNumber
) {
	var self = this;
	var common = Teleopti.MyTimeWeb.Common;
	var userTexts = common.GetUserTexts();
	var constants = common.Constants;

	self.title = ko.observable(layer.Title);
	self.hasMeeting = ko.computed(function() {
		return layer.Meeting !== null;
	});

	self.timeSpan = ko.computed(function() {
		var originalTimespan = layer.TimeSpan;
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
	});

	self.tooltipText = ko.computed(function() {
		var tooltipContent = '<div>{0}</div>'.format(self.title());
		if (!self.hasMeeting()) {
			return tooltipContent + self.timeSpan();
		}

		var meetingDescription = layer.Meeting.Description;
		if (meetingDescription && meetingDescription.length > 200) {
			meetingDescription = meetingDescription.substring(0, 200) + '...';
		}

		var text = (
			"<div>{0}</div><div style='text-align: left'>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{1}</i> {2}</div>" +
			"<div class='tooltip-wordwrap' style='overflow: hidden'><i>{3}</i> {4}</div>" +
			"<div class='tooltip-wordwrap' style='white-space: normal'><i>{5}</i> {6}</div>" +
			'</div>'
		).format(
			self.timeSpan(),
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

		return tooltipContent + text;
	});

	self.backgroundColor = ko.observable(Teleopti.MyTimeWeb.Common.ConvertColorToRGB(layer.Color));

	self.textColor = ko.computed(function() {
		if (layer.Color !== null && layer.Color !== undefined) {
			var backgroundColor = self.backgroundColor();
			return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
		}
		return 'black';
	});

	self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
	self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
	self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
	self.isOvertimeAvailability = ko.observable(layer.IsOvertimeAvailability);
	self.isOvertime = layer.IsOvertime;

	var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();
	self.top = ko.computed(function() {
		if (useFixedContainerHeight) {
			return Math.round(scheduleHeight * self.startPositionPercentage()) + (offset || 0);
		} else if (timelineStart) {
			var milisecondsToTimeLineStart = 0;
			if (timelineStart.indexOf('T') > -1) {
				milisecondsToTimeLineStart = moment(layer.StartTime) - moment(timelineStart);
			} else {
				var startStr = timelineStart.length == 10 ? timelineStart.slice(-8, -3) : timelineStart.slice(0, 5);

				milisecondsToTimeLineStart = moment.duration(layer.StartTime.split('T')[1]) - moment.duration(startStr);
				if (!moment(layer.StartTime.split('T')[0]).isSame(moment(selectedDate), 'day')) {
					milisecondsToTimeLineStart += 24 * 60 * 60 * 1000;
				}
			}

			var top =
				parseInt(milisecondsToTimeLineStart / (60 * 1000)) +
				(milisecondsToTimeLineStart % (60 * 1000)) +
				(offset || 0);
			return top;
		}
	});

	self.height = ko.computed(function() {
		var bottom = 0;

		if (useFixedContainerHeight) {
			bottom = Math.round(scheduleHeight * self.endPositionPercentage()) + 1 + (offset || 0);
		} else if (timelineStart) {
			var bottomDurationInMiliseconds = 0;
			if (timelineStart.indexOf('T') > -1) {
				bottomDurationInMiliseconds = moment(layer.EndTime) - moment(timelineStart);
			} else {
				var startStr = timelineStart.length == 10 ? timelineStart.slice(-8, -3) : timelineStart.slice(0, 5);

				bottomDurationInMiliseconds = moment.duration(layer.EndTime.split('T')[1]) - moment.duration(startStr);
				if (!moment(layer.EndTime.split('T')[0]).isSame(moment(selectedDate), 'day')) {
					bottomDurationInMiliseconds += 24 * 60 * 60 * 1000;
				}
			}

			bottom =
				parseInt(bottomDurationInMiliseconds / (60 * 1000)) +
				(bottomDurationInMiliseconds % (60 * 1000)) +
				(offset || 0);
		}

		var top = self.top();

		return bottom > top ? bottom - top : 0;
	});
	self.topPx = ko.computed(function() {
		return self.top() + 'px';
	});
	self.widthPx = ko.computed(function() {
		return getWidth(layer.IsOvertimeAvailability, parent && parent.probabilities(), layersOnMobile);
	});
	self.heightPx = ko.computed(function() {
		return self.height() + 'px';
	});
	self.overTimeLighterBackgroundStyle = ko.computed(function() {
		var getLumi = function(cstring) {
			var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
			if (!matched) return null;
			return (
				(299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) /
				1000
			);
		};

		var lightColor = '#00ffff';
		var darkColor = '#795548';
		var backgroundColor = common.RGBTohex(self.backgroundColor());
		var useLighterStyle =
			Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) >
			Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

		return useLighterStyle;
	});

	self.overTimeDarkerBackgroundStyle = ko.computed(function() {
		return !self.overTimeLighterBackgroundStyle();
	});

	self.styleJson = ko.computed(function() {
		return {
			top: self.topPx,
			width: self.widthPx,
			height: self.heightPx,
			color: self.textColor,
			'background-size': self.isOvertime ? '11px 11px' : 'initial',
			'background-color': self.backgroundColor
		};
	});

	self.heightDouble = ko.computed(function() {
		return scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
	});
	self.showTitle = ko.computed(function() {
		return self.heightDouble() > constants.pixelToDisplayTitle;
	});
	self.showDetail = ko.computed(function() {
		return self.heightDouble() > constants.pixelToDisplayAll;
	});
};

function getWidth(isOvertimeAvailability, probabilities, layersOnMobile) {
	var width;
	if (isOvertimeAvailability) {
		width = 20 + '%';
	} else if (probabilities && probabilities.length > 0 && layersOnMobile) {
		width = 'calc(100% - 28px)'; //MobileDayView.css .mobile-start-day .probability-vertical-bar{width: 28px;}
	} else {
		width = 'calc(' + 98 + '%)';
	}
	return width;
}
