'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', ['TeamScheduleTimeLineFactory', 'PersonSchedule', GroupScheduleFactoryFn]);

function GroupScheduleFactoryFn(timeLineFactory, personSchedule) {

	var groupScheduleFactory = {
		Create: create
	};

	function create(groupSchedules, queryDateMoment, useNextDayScheduleData, maxPeriodInHour) {
		var maximumViewRange = {
			startMoment: queryDateMoment.clone().startOf('day'),
			endMoment: queryDateMoment.clone().startOf('day').add(maxPeriodInHour ? maxPeriodInHour : 30, 'hour')
		};
		
		var schedulesInRange = groupSchedules.filter(function(schedule) {
			return isScheduleWithinViewRange(schedule, queryDateMoment, maximumViewRange, useNextDayScheduleData);
		});

		var extraSchedules = groupSchedules.filter(function(schedule) {
			return schedulesInRange.indexOf(schedule) < 0;
		});
		
		var timeLine = timeLineFactory.Create(schedulesInRange, queryDateMoment, maximumViewRange);
		return {
			TimeLine: timeLine,
			Schedules: createSchedulesFromGroupSchedules(schedulesInRange, timeLine, extraSchedules)
		};
	}

	function createSchedulesFromGroupSchedules(groupSchedules, timeLine, extraSchedules) {
		var existedSchedulesDictionary = {};
		var schedules = [];

		groupSchedules.forEach(function(schedule) {
			var existedPersonSchedule = existedSchedulesDictionary[schedule.PersonId];
			if (existedPersonSchedule == null) {
				var personScheduleVm = personSchedule.Create(schedule, timeLine);
				existedSchedulesDictionary[schedule.PersonId] = personScheduleVm;
				schedules.push(personScheduleVm);
			} else {
				existedPersonSchedule.Merge(schedule, timeLine);
			}
		});

		extraSchedules.forEach(function(schedule) {
			if (existedSchedulesDictionary[schedule.PersonId] && existedSchedulesDictionary[schedule.PersonId].MergeExtra) {
				existedSchedulesDictionary[schedule.PersonId].MergeExtra(schedule, timeLine);
			}
		});

		return schedules;
	}

	function isScheduleWithinViewRange(schedule, queryDateMoment, maximumViewRange, useNextDayScheduleData) {
		var scheduleDateMoment = moment(schedule.Date);

		if (scheduleDateMoment.isSame(queryDateMoment, 'day')) return true;

		if (scheduleDateMoment.isBefore(queryDateMoment, 'day')) {
			if (schedule.Projection.length > 0) {
				var endTimes = schedule.Projection.map(function(p) {
					return moment(p.End);
				});
				return endTimes.reduce(function(prev, cur) {
					if (!prev) return cur;
					if (prev < cur) return cur;
					return prev;
				}).isAfter(maximumViewRange.startMoment);
			} else if (schedule.DayOff) {
				return moment(schedule.DayOff.End).isAfter(maximumViewRange.startMoment);
			} else {
				return false;
			}
		}

		if (scheduleDateMoment.isAfter(queryDateMoment, 'day')) {
			if (!useNextDayScheduleData) return false;

			if (schedule.DayOff) {
				return moment(schedule.DayOff.Start).isBefore(maximumViewRange.endMoment);
			} else if (schedule.Projection.length > 0) {
				var startTimes = schedule.Projection.map(function(p) {
					return moment(p.Start);
				});

				var time = startTimes.reduce(function(prev, cur) {
					if (!cur) return prev;
					if (cur.isBefore(prev)) return cur;
					return prev;
				});

				return time.isBefore(maximumViewRange.endMoment);
			} else {
				return false;
			}
		}
	}

	return groupScheduleFactory;
}