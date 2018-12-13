(function () {
	'use strict';

	angular.module('wfm.teamSchedule').factory('TeamScheduleTimeLineFactory',
		[
			'ShiftHelper',
			'CurrentUserInfo',
			TeamScheduleTimeLineFactory]);

	function TeamScheduleTimeLineFactory(shiftHelper, currentUserInfo) {

		var timeLineFactory = {
			Create: create
		};

		var defaultViewRange = {
			start: 0,
			end: 2880
		};

		function create(groupSchedules, queryDate, maximumViewRange) {
			var hourPoints = [];

			var baseDate = queryDate.clone().startOf("day");
			if (maximumViewRange) {
				defaultViewRange.start = maximumViewRange.startMoment.diff(baseDate, 'minute');
				defaultViewRange.end = maximumViewRange.endMoment.diff(baseDate, 'minute');
			}

			var start = startMinutes(groupSchedules, baseDate);
			var end = endMinutes(groupSchedules, baseDate);
			var percentPerMinute = calculateLengthPercentPerMinute(start, end);
			var timePoint = start;
			var isLabelVisibleEvenly = percentPerMinute < 0.1; // it means hours less that 16.
			var isLabelHidden = false;

			while (timePoint < end + 1) {
				hourPoints.push(new hourPointViewModel(baseDate, timePoint, start, percentPerMinute, isLabelHidden && isLabelVisibleEvenly));
				timePoint = shiftHelper.MinutesAddHours(timePoint, 1);
				isLabelHidden = !isLabelHidden;
			}

			var timeLineViewModel = {
				Offset: baseDate,
				StartMinute: start,
				EndMinute: end,
				HourPoints: hourPoints,
				LengthPercentPerMinute: percentPerMinute,
				MaximumViewRange: maximumViewRange
			};

			return timeLineViewModel;
		}

		function getMinStartTimeInMinute(schedules, baseDate) {
			if (schedules.length === 0)
				return undefined;

			var availibleProjectionsTodayAndYesterday = [];
			schedules.forEach(function (personSchedule) {
				availibleProjectionsTodayAndYesterday = availibleProjectionsTodayAndYesterday.concat(personSchedule.Projection);
			});

			if (availibleProjectionsTodayAndYesterday.length === 0) {
				return undefined;
			}

			var sortedProjections = availibleProjectionsTodayAndYesterday.sort(compareProjectionByStartTime);

			var minStartOrMaxEndProjection = sortedProjections[0];
			return minStartOrMaxEndProjection.StartMoment.diff(baseDate, 'minutes');
		};

		function getMaxEndTimeInMinute(schedules, baseDate) {
			if (schedules.length === 0)
				return undefined;

			var availibleProjections = [];
			schedules.forEach(function (personSchedule) {
				availibleProjections = availibleProjections.concat(personSchedule.Projection);
			});

			if (availibleProjections.length === 0) {
				return undefined;
			}

			var sortedProjections = availibleProjections.sort(compareProjectionByEndTime);

			var minStartOrMaxEndProjection = sortedProjections[0];
			return minStartOrMaxEndProjection.EndMoment
				.diff(baseDate, 'minutes');
		};

		function startMinutes(groupSchedule, baseDate) {
			var start = getMinStartTimeInMinute(groupSchedule, baseDate);

			if (angular.isUndefined(start) || start >= 1440) // 1440 means 0:00 of next day, to make sure we alwayse show timelines for current day
				return shiftHelper.MinutesForHourOfDay(8);

			start = start > defaultViewRange.start ? start : defaultViewRange.start;

			if ((start !== defaultViewRange.start) && (start % 60 === 0)) {
				start = shiftHelper.MinutesAddHours(start, -1);
			}

			return shiftHelper.MinutesStartOfHour(start);
		};

		function endMinutes(groupSchedule, baseDate) {

			var end = getMaxEndTimeInMinute(groupSchedule, baseDate);

			if (angular.isUndefined(end) || end <= 0) // 0 means 0:00 of current day, to make sure we alwayse show timelines for current day
				return shiftHelper.MinutesForHourOfDay(16);

			end = end < defaultViewRange.end ? end : defaultViewRange.end;

			if (end % 60 === 0 && end !== defaultViewRange.end) {
				end = shiftHelper.MinutesAddHours(end, 1);
			}

			return shiftHelper.MinutesEndOfHour(end);
		};

		function compareProjectionByStartTime(currentProjection, nextProjection) {
			var currentProjectionStartTime = currentProjection.StartMoment;
			var nextProjectionStartTime = nextProjection.StartMoment;

			if (currentProjectionStartTime > nextProjectionStartTime)
				return 1;
			if (currentProjectionStartTime < nextProjectionStartTime)
				return -1;

			return 0;
		};

		function compareProjectionByEndTime(currentProjection, nextProjection) {
			var currentProjectionEndTime = currentProjection.EndMoment;
			var nextProjectionEndTime = nextProjection.EndMoment;

			if (currentProjectionEndTime > nextProjectionEndTime)
				return -1;
			if (currentProjectionEndTime < nextProjectionEndTime)
				return 1;

			return 0;
		};

		function calculateLengthPercentPerMinute(start, end) {
			var lengthInMin = end - start;
			return lengthInMin > 0 ? new Number(100 / lengthInMin).toFixed(3) : 0;
		};

		function hourPointViewModel(baseDate, minutes, start, percentPerMinute, isLabelHidden) {
			var dateTimeFormat = currentUserInfo.CurrentUserInfo().DateTimeFormat || {};
			var shortTimePattern = dateTimeFormat.ShortTimePattern;
			var time = baseDate.clone().startOf('day').add(minutes, 'minutes');

			var isCurrentDay = minutes >= 0 && minutes < 1440;
			var isNextDay = minutes >= 1440;

			var formattedTime = isCurrentDay ?
				time.format(shortTimePattern)
				: (isNextDay ? time.format(shortTimePattern) + " +1" : time.format(shortTimePattern) + " -1");

			this.TimeLabel = formattedTime;
			this.IsLabelVisible = !isLabelHidden;
			this.IsCurrentDay = isCurrentDay;
			this.Position = function () {
				var timeLineStartMinutes = minutes - start;
				var position = timeLineStartMinutes * percentPerMinute;
				return position;
			};
			this.Width = function () {
				return 60 * percentPerMinute;
			}
			
		}

		return timeLineFactory;
	}
}());
