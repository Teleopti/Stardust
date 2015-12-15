'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', ['CurrentUserInfo', 'TeamScheduleTimeLineFactory', 'PersonSchedule',

	function (currentUserInfo, timeLineFactory, personSchedule) {

		var groupScheduleFactory = {
			Create: create
		};

		function create(groupSchedules, queryDate) {

			var scheduleTimeLine = timeLineFactory.Create(groupSchedules, queryDate);
			

			function createSchedulesFromGroupSchedules(groupSchedules, timeLine) {
				var existedSchedulesDictionary = {};
				var schedules = [];

				groupSchedules.forEach(function (schedule) {

					var scheduleDateInUserTimeZone = moment.tz(schedule.Date, currentUserInfo.DefaultTimeZone);
					var isOverNightShift = scheduleDateInUserTimeZone < queryDate;
					var existedPersonSchedule = existedSchedulesDictionary[schedule.PersonId];

					if (existedPersonSchedule == null) {
						existedSchedulesDictionary[schedule.PersonId] = schedule;
						schedules.push(personSchedule.Create(schedule, timeLine, isOverNightShift));
					} else {
						existedPersonSchedule.Merge(schedule, timeLine, isOverNightShift);
					}
				});

				return schedules;
			}

			return {
				TimeLine: scheduleTimeLine,
				Schedules: createSchedulesFromGroupSchedules(groupSchedules, scheduleTimeLine)
			};
		}


		return groupScheduleFactory;
	}
]);
