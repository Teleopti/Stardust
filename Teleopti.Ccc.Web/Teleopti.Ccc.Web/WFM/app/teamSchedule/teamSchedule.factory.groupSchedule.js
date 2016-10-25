'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', ['TeamScheduleTimeLineFactory', 'PersonSchedule',

	function (timeLineFactory, personSchedule) {

		var groupScheduleFactory = {
			Create: create
		};

		function create(groupSchedules, queryDateMoment, useNextDayScheduleData) {
			var schedulesInRange = groupSchedules.filter(function(schedule) {
				return isScheduleWithinViewRange(schedule, queryDateMoment, useNextDayScheduleData);
			});
			var timeLine = timeLineFactory.Create(schedulesInRange, queryDateMoment);
			return {
				TimeLine: timeLine,
				Schedules: createSchedulesFromGroupSchedules(schedulesInRange, timeLine)
			};
		}

		function createSchedulesFromGroupSchedules(groupSchedules, timeLine) {
			var existedSchedulesDictionary = {};
			var schedules = [];

			groupSchedules.forEach(function (schedule) {
				var existedPersonSchedule = existedSchedulesDictionary[schedule.PersonId];
				if (existedPersonSchedule == null) {
					var personScheduleVm = personSchedule.Create(schedule, timeLine);
					existedSchedulesDictionary[schedule.PersonId] = personScheduleVm;
					schedules.push(personScheduleVm);
				} else {
					existedPersonSchedule.Merge(schedule, timeLine);
				}
			});
			return schedules;
		}

		var maximumViewRange = {
			startMoment: moment().startOf('day'),
			endMoment: moment().startOf('day').add(32, 'hour')
		};

		function isScheduleWithinViewRange(schedule, queryDateMoment, useNextDayScheduleData) {
			var scheduleDateMoment = moment(schedule.Date);

			if (scheduleDateMoment.isSame(queryDateMoment, 'day')) return true;

			if (scheduleDateMoment.isBefore(queryDateMoment, 'day')) {
				if (schedule.DayOff) {
					return moment(schedule.DayOff.End).isAfter(maximumViewRange.startMoment);
				} else if (schedule.Projection.length > 0) {
					var endTimes = schedule.Projection.map(function (p) {
						return moment(p.End);
					});
					return endTimes.reduce(function (prev, cur) {
							if (!prev) return cur;
							if (prev < cur) return cur;
							return prev;
						})
						.isAfter(maximumViewRange.startMoment);
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

					return startTimes.reduce(function (prev, cur) {
							if (!prev) return cur;
							if (cur < prev) return cur;
							return prev;
						})
						.isBefore(maximumViewRange.endMoment);
				} else {
					return false;
				}
			}
		}

		return groupScheduleFactory;
	}
]);
