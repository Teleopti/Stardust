'use strict';

angular.module('wfm.teamSchedule').factory('GroupScheduleFactory', [
	'CurrentUserInfo', 'ShiftHelper', 'TimeLine', 'PersonSchedule',
	function(currentUserInfo, shiftHelper, timeLine, personSchedule) {
		var groupScheduleViewModel = {};

		groupScheduleViewModel.Create = function(groupSchedules, baseDate) {
			var scheduleTimeLine = timeLine;
			groupScheduleViewModel.TimeLine = scheduleTimeLine.Create(groupSchedules, baseDate);

			var schedules = [];
			angular.forEach(groupSchedules, function(schedule) {
				var scheduleVm = personSchedule;
				schedules.push(scheduleVm.Create(schedule, baseDate, groupScheduleViewModel.TimeLine));
			});

			groupScheduleViewModel.Schedules = schedules;
		}

		return groupScheduleViewModel;
	}
]);