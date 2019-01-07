'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory',
	['TeamScheduleTimeLineFactory',
		'PersonSchedule',
		'serviceDateFormatHelper',
		'$filter',
		GroupScheduleFactory]);

function GroupScheduleFactory(timeLineFactory, personSchedule, serviceDateFormatHelper, $filter) {

	var groupScheduleFactory = {
		Create: create
	};

	function create(rawSchedules, queryDate, timezone, maximumHours) {

		var timezoneAdjustedSchedules = rawSchedules.map(function (schedule) {
			return convertScheduleToTimezone(schedule, queryDate, timezone);
		});

		var queryDateInTimezone = moment.tz(queryDate, timezone);

		var maximumViewRange = {
			startMoment: queryDateInTimezone.clone().startOf('day'),
			endMoment: queryDateInTimezone.clone().startOf('day').add(maximumHours || 30, 'hour')
		};

		var schedulesInRange = timezoneAdjustedSchedules.filter(function (schedule) {
			return isScheduleWithinViewRange(schedule, queryDate, maximumViewRange);
		});

		var extraSchedules = timezoneAdjustedSchedules.filter(function (schedule) {
			return schedulesInRange.indexOf(schedule) < 0;
		});

		var timeline = timeLineFactory.Create(schedulesInRange, queryDateInTimezone, maximumViewRange);

		return {
			TimeLine: timeline,
			Schedules: createSchedulesFromGroupSchedules(schedulesInRange, timeline, extraSchedules)
		};
	}

	function convertScheduleToTimezone(schedule, queryDate, timezone) {
		var copiedSchedule = angular.copy(schedule);

		function covertToTimezone(item) {
			item.StartMoment = moment.tz(item.StartInUtc, 'ETC/UTC').tz(timezone);
			item.Start = serviceDateFormatHelper.getDateTime(item.StartMoment);
			item.EndMoment = moment.tz(item.EndInUtc, 'ETC/UTC').tz(timezone);
			item.End = serviceDateFormatHelper.getDateTime(item.EndMoment);
		}

		function covertSummaryToTimezone(summary) {
			covertToTimezone(summary);
			var isNotSameDay = !summary.StartMoment.isSame(summary.EndMoment, 'day')
				|| !summary.StartMoment.isSame(moment.tz(queryDate, timezone), 'day');
			summary.TimeSpan = getFormatedTimeSpan(summary.StartMoment, summary.EndMoment, isNotSameDay);
			return summary;
		}

		function convertProjectionToTimezone(projection) {
			covertToTimezone(projection);
			var isNotSameDay = !projection.StartMoment.isSame(projection.EndMoment, 'day');
			projection.TimeSpan = getFormatedTimeSpan(projection.StartMoment, projection.EndMoment, isNotSameDay);
			return projection;
		}

		angular.forEach(copiedSchedule.Projection, convertProjectionToTimezone);

		var underlyingScheduleSummary = copiedSchedule.UnderlyingScheduleSummary;
		if (!!underlyingScheduleSummary) {
			angular.forEach(underlyingScheduleSummary.PersonalActivities, covertSummaryToTimezone);
			angular.forEach(underlyingScheduleSummary.PersonPartTimeAbsences, covertSummaryToTimezone);
			angular.forEach(underlyingScheduleSummary.PersonMeetings, covertSummaryToTimezone);
		}

		if (copiedSchedule.DayOff) {
			covertToTimezone(copiedSchedule.DayOff);
		}
		return copiedSchedule;
	}

	function getFormatedTimeSpan(startMoment, endMoment, isNotSameDay) {
		if (isNotSameDay) {
			return startMoment.format("L LT") + ' - ' + endMoment.format("L LT");
		}
		return startMoment.format("LT") + ' - ' + endMoment.format("LT");
	}


	function createSchedulesFromGroupSchedules(schedules, timeLine, extraSchedules) {
		var existedSchedulesDictionary = {};
		var scheduleVms = [];

		schedules.forEach(function (schedule, index) {
			var existedPersonSchedule = existedSchedulesDictionary[schedule.PersonId];
			if (existedPersonSchedule == null) {
				var personScheduleVm = personSchedule.Create(schedule, timeLine, index);
				existedSchedulesDictionary[schedule.PersonId] = personScheduleVm;
				scheduleVms.push(personScheduleVm);
			} else {
				existedPersonSchedule.Merge(schedule, timeLine);
			}
		});

		extraSchedules.forEach(function (schedule) {
			if (existedSchedulesDictionary[schedule.PersonId] && existedSchedulesDictionary[schedule.PersonId].MergeExtra) {
				existedSchedulesDictionary[schedule.PersonId].MergeExtra(schedule, timeLine);
			}
		});

		return scheduleVms;
	}

	function isScheduleWithinViewRange(schedule, queryDate, maximumViewRange) {
		if (queryDate == schedule.Date) return true;

		if (schedule.Date < queryDate) {
			if (schedule.Projection && schedule.Projection.length > 0) {
				var endTimes = schedule.Projection.map(function (p) {
					return p.EndMoment.clone();
				});
				return endTimes.reduce(function (prev, cur) {
					if (!prev) return cur;
					if (prev < cur) return cur;
					return prev;
				}).isAfter(maximumViewRange.startMoment);
			}
			if (schedule.DayOff) {
				return schedule.DayOff.EndMoment.isAfter(maximumViewRange.startMoment);
			}
			return false;
		}

		if (schedule.Date > queryDate) {
			if (schedule.Projection && schedule.Projection.length > 0) {
				var startTimes = schedule.Projection.map(function (p) {
					return p.StartMoment.clone();
				});

				return startTimes.reduce(function (prev, cur) {
					if (!cur) return prev;
					if (cur.isBefore(prev)) return cur;
					return prev;
				}).isBefore(maximumViewRange.endMoment);
			}
			if (schedule.DayOff) {
				return schedule.DayOff.StartMoment.isBefore(maximumViewRange.endMoment);
			}
			return false;
		}
	}


	return groupScheduleFactory;
}