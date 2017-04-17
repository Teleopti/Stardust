(function() {
	'use strict';

	angular.module('wfm.teamSchedule').service('belongsToDateDecider', belongsToDateDecider);

	belongsToDateDecider.$inject = ['$filter'];

	function belongsToDateDecider($filter) {

		var self = this;

		self.decideBelongsToDate = decideBelongsToDate;
		self.normalizePersonScheduleVm = normalizePersonScheduleVm;
		self.checkTimeRangeAllowedForIntradayAbsence = checkTimeRangeAllowedForIntradayAbsence;
		self.decideBelongsToDateForOvertimeActivity = decideBelongsToDateForOvertimeActivity;

		function decideBelongsToDateForOvertimeActivity(targetTimeRange, normalizedScheduleDataArray) {
			var intersectedShiftDays = normalizedScheduleDataArray.filter(function (day) {
				return day.shiftRange && timeRangeIntersect(day.shiftRange, targetTimeRange);
			});

			if (intersectedShiftDays.length > 0) return intersectedShiftDays[0].date;

			var matchedDates = normalizedScheduleDataArray.filter(function(d) {
				return d.timeRange.startTime <= targetTimeRange.startTime && targetTimeRange.startTime < d.timeRange.endTime;
			}).map(function(d) {
				return d.date;
			});

			return matchedDates[0];
		}

		function checkTimeRangeAllowedForIntradayAbsence(targetTimeRange, normalizedScheduleDataArray) {
			return normalizedScheduleDataArray.some(function(day) {
				return day.shiftRange &&
					targetTimeRange.startTime.isSameOrAfter(day.shiftRange.startTime) &&
					targetTimeRange.endTime.isSameOrBefore(day.shiftRange.endTime);
			});
		}

		function decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, currentDate) {		

			var intersectedShiftDays = normalizedScheduleDataArray.filter(function(day) {
				return day.shiftRange && timeRangeIntersect(day.shiftRange, targetTimeRange);
			});

			if (intersectedShiftDays.length > 1) return null;
			if (intersectedShiftDays.length === 1) {
				if (mergedRangeIsNotTooLong(targetTimeRange, intersectedShiftDays[0].shiftRange) 
					&& !rangeStartsBeforeDay(targetTimeRange, intersectedShiftDays[0].timeRange)) {
					return intersectedShiftDays[0].date;
				} else {
					return null;
				}				
			}

			var startInEmptyDays = normalizedScheduleDataArray.filter(function (day) {				
				return day.date === currentDate && !day.shiftRange && targetTimeRange.startTime >= day.timeRange.startTime && targetTimeRange.startTime < day.timeRange.endTime;
			});

			if (startInEmptyDays.length !== 1) return null;
			return startInEmptyDays[0].date;
		}

		function normalizePersonScheduleVm(personScheduleVm, currentTimezone) {
			
			var dates = [moment(personScheduleVm.Date).add(-1, 'day').format('YYYY-MM-DD'), personScheduleVm.Date, moment(personScheduleVm.Date).add(1, 'day').format('YYYY-MM-DD')];

			var result = dates.map(function (date) {
				var dayStart = moment(date).startOf('day');
				var dayEnd = moment(date).add(24, 'hour');
				var timeRangeForDate = {
					startTime: moment($filter('timezone')(dayStart.format('YYYY-MM-DD HH:mm'), currentTimezone, personScheduleVm.Timezone.IanaId)),
					endTime: moment($filter('timezone')(dayEnd.format('YYYY-MM-DD HH:mm'), currentTimezone, personScheduleVm.Timezone.IanaId))
				}
				return {
					date: date,
					timeRange: timeRangeForDate,
					shiftRange: null
				}
			});

			if (personScheduleVm.Shifts && personScheduleVm.Shifts.length > 0) {
				angular.forEach(personScheduleVm.Shifts, function (shift) {
					if (!shift.ProjectionTimeRange) {
						return;
					}
					var shiftStart = moment(shift.ProjectionTimeRange.Start);
					var shiftEnd = moment(shift.ProjectionTimeRange.End);
					var index = dates.indexOf(shift.Date);
					if (index >= 0) {
						result[index].shiftRange = {
							startTime: shiftStart,
							endTime: shiftEnd
						};
					}
				});
			}

			
			return result;			
		}

		function mergedRangeIsNotTooLong(timeRangeA, timeRangeB) {
			var allowShiftTotalMinutes = 36 * 60;
			var startTime = timeRangeA.startTime < timeRangeB.startTime ? timeRangeA.startTime : timeRangeB.startTime;
			var endTime = timeRangeA.endTime < timeRangeB.endTime ? timeRangeB.endTime : timeRangeA.endTime;
			return endTime.diff(startTime, 'minute') <= allowShiftTotalMinutes;
		}

		function rangeStartsBeforeDay(targetTimeRange, dayTimeRange) {
			return targetTimeRange.startTime < dayTimeRange.startTime;
		}

		function timeRangeIntersect(timeRangeA, timeRangeB) {
			return (timeRangeA.startTime.isSameOrBefore(timeRangeB.startTime, 'minute') && timeRangeA.endTime.isSameOrAfter( timeRangeB.startTime, 'minute')) ||
				(timeRangeB.startTime.isSameOrBefore(timeRangeA.startTime, 'minute') && timeRangeB.endTime.isSameOrAfter(timeRangeA.startTime, 'minute'));
		}
	}

})();
