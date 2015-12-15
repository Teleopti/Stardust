'use strict';

(function () {
	
	angular.module('wfm.teamSchedule').factory('TeamScheduleTimeLineFactory', ['CurrentUserInfo', 'ShiftHelper', TeamScheduleTimeLineFactory]);

	function TeamScheduleTimeLineFactory(currentUserInfo, shiftHelper) {

		var timeLineFactory = {
			Create: create
		};

		function create(groupSchedules, utcQueryDate) {
			var hourPoints = [];

			var utcBaseDate = utcQueryDate.startOf("day");
			var start = startMinutes(groupSchedules, utcBaseDate);
			var end = endMinutes(groupSchedules, utcBaseDate);
			var percentPerMinute = calculateLengthPercentPerMinute(start, end);
			var timePoint = start;
			var isLabelVisibleEvenly = percentPerMinute < 0.1;
			var isLabelHidden = false;

			while (timePoint < end + 1) {
				hourPoints.push(new hourPointViewModel(utcBaseDate, timePoint, start, percentPerMinute, isLabelHidden && isLabelVisibleEvenly));
				timePoint = shiftHelper.MinutesAddHours(timePoint, 1);
				isLabelHidden = !isLabelHidden;
			}

			var timeLineViewModel = {
				Offset: utcBaseDate,
				StartMinute: start,
				EndMinute: end,
				HourPoints: hourPoints,
				LengthPercentPerMinute: percentPerMinute
			};
			return timeLineViewModel;
		}

		function startMinutes(groupSchedule, baseDate) {

			var start = getMinStartTimeOrMaxEndTimeInMinuteFromSchedules(groupSchedule, baseDate, true);

			// If exists any schedule started from yesterday, set timeline start from 00:00;
			start = start > 0 ? start : 0;

			if (start === undefined)
				return shiftHelper.MinutesForHourOfDay(8);
			if (start > 60 && (start % 60 == 0)) {
				start = shiftHelper.MinutesAddHours(start, -1);
			}
				
			return shiftHelper.MinutesStartOfHour(start);
		};

		function endMinutes(groupSchedule, baseDate) {

			var end = getMinStartTimeOrMaxEndTimeInMinuteFromSchedules(groupSchedule, baseDate, false);

			if (end === undefined)
				return shiftHelper.MinutesForHourOfDay(16);
			if (end % 60 == 0) {
				end = shiftHelper.MinutesAddHours(end, 1);
			}

			return shiftHelper.MinutesEndOfHour(end);
		};

		function getMinStartTimeOrMaxEndTimeInMinuteFromSchedules(schedules, baseDate, isStartTime) {

			if (schedules.length === 0)
				return undefined;

			var availibleProjections= [];
			schedules.forEach(function (personSchedule) {
				if (hasAvailibleProjections(personSchedule, baseDate)) {
					availibleProjections = availibleProjections.concat(personSchedule.Projection);
				}
			});

			var sortedProjections = isStartTime
				? availibleProjections.sort(compareProjectionByStartTime)
				: availibleProjections.sort(compareProjectionByEndTime);

			var minStartOrMaxEndProjection = sortedProjections[0];
			return isStartTime
				? moment(minStartOrMaxEndProjection.Start).diff(baseDate, 'minutes')
				: moment(minStartOrMaxEndProjection.Start).add(minStartOrMaxEndProjection.Minutes, 'minutes').diff(baseDate, 'minutes');
		};

		function hasAvailibleProjections(personSchedule, baseDate) {
			var scheduleDate = moment(personSchedule.Date);
			var hasProjections = personSchedule.Projection != undefined && personSchedule.Projection.length > 0;
			var isScheduleBelongsToTheDateBefore = scheduleDate.diff(baseDate, "days") <= 0;
			return hasProjections && isScheduleBelongsToTheDateBefore;
		};

		function compareProjectionByStartTime(currentProjection, nextProjection) {

			var currentProjectionStartTime = moment(currentProjection.Start);
			var nextProjectionStartTime = moment(nextProjection.Start);

			if (currentProjectionStartTime > nextProjectionStartTime)
				return 1;
			if (currentProjectionStartTime < nextProjectionStartTime)
				return -1;

			return 0;
		};

		function compareProjectionByEndTime(currentProjection, nextProjection) {

			var currentProjectionEndTime = moment(currentProjection.Start).add(currentProjection.Minutes, 'minutes');
			var nextProjectionEndTime = moment(nextProjection.Start).add(nextProjection.Minutes, 'minutes');

			if (currentProjectionEndTime > nextProjectionEndTime)
				return -1;
			if (currentProjectionEndTime < nextProjectionEndTime)
				return 1;

			return 0;
		};

		function calculateLengthPercentPerMinute(start, end) {
			var lengthInMin = end - start;
			return lengthInMin > 0 ? 100 / lengthInMin : 0;
		};

		function hourPointViewModel(baseDate, minutes, start, percentPerMinute, isLabelHidden) {
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



		return timeLineFactory;
	}
}());
