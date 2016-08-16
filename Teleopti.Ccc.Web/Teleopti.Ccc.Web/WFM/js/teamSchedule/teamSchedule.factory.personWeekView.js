(function() {
	'use strict';

	angular.module('wfm.teamSchedule').factory('PersonScheduleWeekViewCreator', PersonScheduleWeekViewCreator);
	PersonScheduleWeekViewCreator.$inject = [];

	function PersonScheduleWeekViewCreator() {
		function createPersonWeekViewModel(personWeek) {			
			var days = [];
			angular.forEach(personWeek.DaySchedules, function(day) {
				var dayViewModel = {
					date: day.Date,
					summeryTitle: day.Title,
					summeryTimeSpan: day.TimeSpan,
					isDayOff: day.IsDayOff,
					color: day.Color
				};
				days.push(dayViewModel);
			});
			var personWeekViewModel = {
				personId: personWeek.PersonId,
				name: personWeek.Name,
				days: days
			}
			return personWeekViewModel;
		}

		function createGroupWeeks(personWeeks) {
			
			var groupWeeks = [];
			angular.forEach(personWeeks, function(personWeek) {
				var personWeekViewModel = createPersonWeekViewModel(personWeek);
				groupWeeks.push(personWeekViewModel);
			});
			return groupWeeks;
		}
		return {
			Create: createGroupWeeks
		};
	}
})()