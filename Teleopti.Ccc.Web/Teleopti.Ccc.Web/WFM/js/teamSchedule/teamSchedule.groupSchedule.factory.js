'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'ShiftHelper', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, shiftHelper, timeLine, personSchedule) {
		var groupScheduleViewModel = {};

		groupScheduleViewModel.Create = function (groupSchedules, queryDate) {
			var scheduleTimeLine = timeLine.Create(groupSchedules, queryDate);

			var schedules = [];
			angular.forEach(groupSchedules, function(schedule) {
				var scheduleVm = personSchedule;
				var existedPersonSchedule = null;
				for (var i = 0; i < schedules.length; i++) {
					if (schedules[i].PersonId === schedule.PersonId) {
						existedPersonSchedule = schedules[i];
						break;
					}
				}

				if (existedPersonSchedule == null) {
					schedules.push(scheduleVm.Create(schedule, scheduleTimeLine));
				} else {
					existedPersonSchedule.Merge(schedule, scheduleTimeLine);
				}
			});

			groupScheduleViewModel.TimeLine = scheduleTimeLine;
			groupScheduleViewModel.Schedules = schedules;
		}

		return groupScheduleViewModel;
	}
]);