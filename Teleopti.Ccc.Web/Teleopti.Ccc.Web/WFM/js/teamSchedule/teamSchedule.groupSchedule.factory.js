'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'ShiftHelper', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, shiftHelper, timeLine, personSchedule) {
		var groupScheduleViewModel = {};

		groupScheduleViewModel.Create = function (groupSchedules, queryDate, canvasSize) {
			var scheduleTimeLine = timeLine;

			var baseDate = queryDate.startOf('day');
			groupScheduleViewModel.TimeLine = scheduleTimeLine.Create(groupSchedules, baseDate, canvasSize);

			var schedules = [];
			angular.forEach(groupSchedules, function(schedule) {
				var scheduleVm = personSchedule;
				var personExisted = false;
				for (var i = 0; i < schedules.length; i++) {
					if (schedules[i].PersonId == schedule.PersonId) {
						personExisted = true;
					}
				}
				if (!personExisted) {
					schedules.push(scheduleVm.Create(schedule, groupScheduleViewModel.TimeLine));
				}
				
			});

			groupScheduleViewModel.Schedules = schedules;
		}

		return groupScheduleViewModel;
	}
]);