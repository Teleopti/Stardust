'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, timeLine, personSchedule) {
		var groupScheduleFactory = {};

		var scheduleSort = function(first, second) {
			var firstSortValue = first.SortValue();
			var firstPersonName = first.Name;

			var secondSortValue = second.SortValue();
			var secondPersonName = second.Name;

			var nameOrder = firstPersonName === secondPersonName ? 0 : (firstPersonName < secondPersonName ? -1 : 1);
			return firstSortValue === secondSortValue ? nameOrder : (firstSortValue < secondSortValue ? -1 : 1);
		}

		groupScheduleFactory.Create = function (groupSchedules, queryDate) {
			var scheduleTimeLine = timeLine.Create(groupSchedules, queryDate);

			var schedules = [];
			angular.forEach(groupSchedules, function(schedule) {
				var existedPersonSchedule = null;
				for (var i = 0; i < schedules.length; i++) {
					if (schedules[i].PersonId === schedule.PersonId) {
						existedPersonSchedule = schedules[i];
						break;
					}
				}

				if (existedPersonSchedule == null) {
					schedules.push(personSchedule.Create(schedule, scheduleTimeLine));
				} else {
					existedPersonSchedule.Merge(schedule, scheduleTimeLine);
				}
			});

			return {
				TimeLine: scheduleTimeLine,
				Schedules: schedules.sort(scheduleSort)
			};
		}

		return groupScheduleFactory;
	}
]);
