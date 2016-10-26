(function() {
	'use strict';

	angular.module('wfm.teamSchedule').service('belongsToDateDecider', belongsToDateDecider);

	belongsToDateDecider.$inject = ['$filter'];

	function belongsToDateDecider($filter) {

		var self = this;

		self.decideBelongsToDate = decideBelongsToDate;
		self.normalizePersonScheduleVm = normalizePersonScheduleVm;


		function decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray) {
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

			var startInEmptyDays = normalizedScheduleDataArray.filter(function(day) {
				return !day.shiftRange && timeInTimeRange(targetTimeRange.startTime, day.timeRange);
			});

			if (startInEmptyDays.length !== 1) return null;
			return startInEmptyDays[0].date;
		}

		function normalizePersonScheduleVm(personScheduleVm, currentTimezone) {
			var result = {
				date: personScheduleVm.Date
			};

			var currentDayStart = moment(personScheduleVm.Date).startOf('day');
			var currentDayEnd = moment(personScheduleVm.Date).add(24, 'hour');

			result.timeRange = {
				startTime: moment($filter('timezone')(currentDayStart.format('YYYY-MM-DD hh:mm'), currentTimezone, personScheduleVm.Timezone)),
				endTime: moment($filter('timezone')(currentDayEnd.format('YYYY-MM-DD hh:mm'), currentTimezone, personScheduleVm.Timezone))
			}

			if (personScheduleVm.Shifts && personScheduleVm.Shifts.length > 0) {
				result.shiftRange = {
					startTime: moment(personScheduleVm.ScheduleStartTime),
					endTime: moment(personScheduleVm.ScheduleEndTime)
				};
			} else {
				result.shiftRange = null;
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
			return (timeRangeA.startTime <= timeRangeB.startTime && timeRangeA.endTime >= timeRangeB.startTime) ||
				(timeRangeB.startTime <= timeRangeA.startTime && timeRangeB.endTime >= timeRangeA.startTime);
		}

		function timeInTimeRange(time, timeRange) {
			return timeRange.startTime <= time && time < timeRange.endTime;
		}

	}

})();
