'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, timeLine, personSchedule) {
		var scheduleSort = function(first, second) {
			var firstSortValue = first.SortValue();
			var firstPersonName = first.Name;

			var secondSortValue = second.SortValue();
			var secondPersonName = second.Name;

			var nameOrder = firstPersonName === secondPersonName ? 0 : (firstPersonName < secondPersonName ? -1 : 1);
			return firstSortValue === secondSortValue ? nameOrder : (firstSortValue < secondSortValue ? -1 : 1);
		}

		var create = function (groupSchedules, queryDate) {
			var scheduleTimeLine = timeLine.Create(groupSchedules, queryDate);
			
			var schedules = [];
			angular.forEach(groupSchedules, function (schedule) {
				var isOverNightShift = false;
				var scheduleDateInUserTimeZone = moment.tz(schedule.Date, currentUserInfo.DefaultTimeZone);
				if (scheduleDateInUserTimeZone < queryDate) {
					isOverNightShift = true;
				}
				var existedPersonSchedule = null;
				for (var i = 0; i < schedules.length; i++) {
					if (schedules[i].PersonId === schedule.PersonId) {
						existedPersonSchedule = schedules[i];
						break;
					}
				}

				if (existedPersonSchedule == null) {
					schedules.push(personSchedule.Create(schedule, scheduleTimeLine, isOverNightShift));
				} else {
					existedPersonSchedule.Merge(schedule, scheduleTimeLine, isOverNightShift);
				}
			});

			return {
				TimeLine: scheduleTimeLine,
				Schedules: schedules
			};
		}

		var groupScheduleFactory = {
			Create: create
		};
		return groupScheduleFactory;
	}
]);
