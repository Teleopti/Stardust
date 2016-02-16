'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', ['CurrentUserInfo', 'TeamScheduleTimeLineFactory', 'PersonSchedule',

	function (currentUserInfo, timeLineFactory, personSchedule) {

		var groupScheduleFactory = {
			Create: create
		};

		function create(groupSchedules, queryDate) {
			var timeLine = timeLineFactory.Create(groupSchedules, queryDate);
			return {
				TimeLine: timeLine,
				Schedules: createSchedulesFromGroupSchedules(groupSchedules, timeLine)
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

		return groupScheduleFactory;
	}
]);
