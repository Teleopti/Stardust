﻿(function () {
	'use strict';
	angular.module('wfm.teamSchedule').factory('TimeLine', ['CurrentUserInfo', 'ShiftHelper', TimeLine]);

	function TimeLine(currentUserInfo, shiftHelper) {
		var startMinutes = function (groupSchedule, baseDate) {
			var start = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				var scheduleDate = moment(personSchedule.Date);
				if (scheduleDate.diff(baseDate, "days") <= 0) {
					allProjections = allProjections.concat(personSchedule.Projection);
				}
			});

			angular.forEach(allProjections, function (projection) {
				var projStartTime = moment(projection.Start);
				var projStartTimeMin = projStartTime.diff(baseDate, 'minutes');

				if (start === undefined || projStartTimeMin < start) {
					start = projStartTimeMin;
				}
			});

			// If exists any schedule started from yesterday, set timeline start from 00:00;
			start = start > 0 ? start : 0;

			if (start === undefined)
				return shiftHelper.MinutesForHourOfDay(8);
			if (start > 60 && (start % 60 == 0)) {
				start = shiftHelper.MinutesAddHours(start, -1);
			}
				

			return shiftHelper.MinutesStartOfHour(start);
		};

		var endMinutes = function (groupSchedule, baseDate) {
			var end = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				var scheduleDate = moment(personSchedule.Date);
				if (scheduleDate.diff(baseDate, "days") <= 0) {
					allProjections = allProjections.concat(personSchedule.Projection);
				}
			});

			angular.forEach(allProjections, function (projection) {
				var projStartTime = moment(projection.Start);
				var projStartTimeMin = projStartTime.diff(baseDate, 'minutes');
				var projEndMinutes = projStartTimeMin + projection.Minutes;
				if (end === undefined || projEndMinutes > end) {
					end = projEndMinutes;
				}
			});

			if (end === undefined)
				return shiftHelper.MinutesForHourOfDay(16);
			if (end % 60 == 0) {
				end = shiftHelper.MinutesAddHours(end, 1);
			}

			return shiftHelper.MinutesEndOfHour(end);
		};

		var calculateLengthPercentPerMinute = function (start, end) {
			var lengthInMin = end - start;
			return lengthInMin > 0 ? 100 / lengthInMin : 0;
		};

		var hourPointViewModel = function (baseDate, minutes, start, percentPerMinute, isLabelHidden) {
			var time = ((baseDate == undefined)
				? moment.tz(currentUserInfo.DefaultTimeZone)
				: moment.tz(baseDate, currentUserInfo.DefaultTimeZone)).startOf('day').add(minutes, 'minutes');

			var formattedTime = time.format('LT');

			var hourPointVm = {
				TimeLabel: formattedTime,
				IsLabelVisible: !isLabelHidden,
				Position: function () {
					var timeLineStartMinutes = minutes - start;
					var position = timeLineStartMinutes * percentPerMinute;
					return position;
				}
			}
			
			return hourPointVm;
		};

		var create = function (groupSchedules, utcQueryDate) {
			var hourPoints = [];

			var utcBaseDate = utcQueryDate.startOf("day");
			var start = startMinutes(groupSchedules, utcBaseDate);
			var end = endMinutes(groupSchedules, utcBaseDate);
			var percentPerMinute = calculateLengthPercentPerMinute(start, end);
			var isLabelVisibleEvenly = percentPerMinute < 0.1 ? true : false;
			var timePoint = start;
			var isLabelHidden = false;
			while (timePoint < end + 1) {
				hourPoints.push(new hourPointViewModel(utcBaseDate, timePoint, start, percentPerMinute, isLabelHidden && isLabelVisibleEvenly));
				timePoint = shiftHelper.MinutesAddHours(timePoint, 1);
				isLabelHidden = !isLabelHidden;
			}

			var timeLine = {
				Offset: utcBaseDate,
				StartMinute: start,
				EndMinute: end,
				HourPoints: hourPoints,
				LengthPercentPerMinute: percentPerMinute
			}
			return timeLine;
		}

		var timeLineFactory = {
			Create: create
		};

		return timeLineFactory;
	}
}());
