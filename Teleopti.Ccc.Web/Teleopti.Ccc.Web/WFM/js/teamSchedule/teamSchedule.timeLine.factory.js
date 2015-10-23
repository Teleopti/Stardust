(function () {
	'use strict';
	angular.module('wfm.teamSchedule').factory('TimeLine', ['CurrentUserInfo', 'ShiftHelper', TimeLine]);

	function TimeLine(currentUserInfo, shiftHelper) {
		var timeLine = {};

		var startMinutes = function (groupSchedule, baseDate) {
			var start = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				allProjections.concat(personSchedule.Projection);
			});

			angular.forEach(allProjections, function (projection) {
				var projStartTime = moment.tz(projection.Start, currentUserInfo.DefaultTimeZone);
				var projStartTimeMin = projStartTime.diff(baseDate, 'minutes');
				var projCutInsideDayStartMinutes = projStartTimeMin >= 0 ? projStartTimeMin : 0;

				if (projection.Date.diff(baseDate) === 0) {
					var projStartMinutes = projCutInsideDayStartMinutes;
					if (start === undefined)
						start = projStartMinutes;
					if (projStartMinutes < start)
						start = projStartMinutes;
				}
			});

			if (start === undefined)
				return shiftHelper.MinutesForHourOfDay(8);

			return shiftHelper.MinutesStartOfHour(start);
		};

		var endMinutes = function (groupSchedule, baseDate) {
			var end = undefined;

			var allProjections = [];
			angular.forEach(groupSchedule, function (personSchedule) {
				allProjections.concat(personSchedule.Projection);
			});

			angular.forEach(allProjections, function (projection) {
				var projStartTime = moment.tz(projection.Start, currentUserInfo.DefaultTimeZone);
				var projStartTimeMin = projStartTime.diff(baseDate, 'minutes');
				var projEndMinutes = projStartTimeMin + projection.Minutes;
				if (end === undefined)
					end = projEndMinutes;
				if (projEndMinutes > end)
					end = projEndMinutes;
			});

			if (end === undefined)
				return shiftHelper.MinutesForHourOfDay(16);

			return shiftHelper.MinutesEndOfHour(end);
		};

		var pixelsPerMinute = function (start, end, widthPixels) {
			var lengthInMin = end - start;
			if (lengthInMin > 0)
				return widthPixels / end - start;
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

		timeLine.Create = function (groupSchedules, baseDate) {
			var hourPoints = [];

			var start = startMinutes(groupSchedules, baseDate);
			var end = endMinutes(groupSchedules, baseDate);

			var timePoint = start;
			while (timePoint < end + 1) {
				hourPoints.push(new hourPointViewModel(baseDate, timePoint, start, 1)); // TODO: Change 1 to pixelPerMinute
				timePoint = shiftHelper.MinutesAddHours(timePoint, 1);
			}
			var timeLineVm = {
				StartMinute: start,
				EndMinute: end,
				HourPoints: hourPoints
			}
			return timeLineVm;
		}

		return timeLine;
	}
}());
