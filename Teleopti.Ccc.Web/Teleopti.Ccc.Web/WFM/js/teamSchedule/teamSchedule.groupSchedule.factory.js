'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, timeLine, personSchedule) {
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
