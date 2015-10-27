(function () {
	'use strict';
	angular.module('wfm.teamSchedule').factory('TimeLine', ['CurrentUserInfo', 'ShiftHelper', TimeLine]);

	function TimeLine(currentUserInfo, shiftHelper) {
		var timeLine = {};

		var startMinutes = function (groupSchedule, baseDate) {
			var start = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				allProjections = allProjections.concat(personSchedule.Projection);
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

			return shiftHelper.MinutesStartOfHour(start);
		};

		var endMinutes = function (groupSchedule, baseDate) {
			var end = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				allProjections = allProjections.concat(personSchedule.Projection);
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

			return shiftHelper.MinutesEndOfHour(end);
		};

		var calculatePixelsPerMinute = function (start, end, widthPixels) {
			var lengthInMin = end - start;
			if (lengthInMin > 0)
				return widthPixels / lengthInMin;
			return 0;
		};

		var hourPointViewModel = function (baseDate, minutes, start, pixelsPerMinute) {
			var time = ((baseDate == undefined)
				? moment.tz(currentUserInfo.DefaultTimeZone)
				: moment.tz(baseDate, currentUserInfo.DefaultTimeZone)).startOf('day').add(minutes, 'minutes');

			var formattedTime = time.format("HH:mm");

			var hourPointVm = {
				Time: formattedTime,
				Pixel: function () {
					var timeLineStartMinutes = minutes - start;
					var pixels = timeLineStartMinutes * pixelsPerMinute;
					return Math.round(pixels);
				}
			}

			return hourPointVm;
		};

		timeLine.Create = function (groupSchedules, utcBaseDate, canvasSize) {
			var hourPoints = [];

			var start = startMinutes(groupSchedules, utcBaseDate);
			var end = endMinutes(groupSchedules, utcBaseDate);
			var pixelsPerMinute = calculatePixelsPerMinute(start, end, canvasSize);

			var timePoint = start;
			while (timePoint < end + 1) {
				hourPoints.push(new hourPointViewModel(utcBaseDate, timePoint, start, pixelsPerMinute));
				timePoint = shiftHelper.MinutesAddHours(timePoint, 1);
			}
			var timeLineVm = {
				Offset: utcBaseDate,
				StartMinute: start,
				EndMinute: end,
				HourPoints: hourPoints,
				PixelsPerMinute: pixelsPerMinute
			}
			return timeLineVm;
		}

		return timeLine;
	}
}());
