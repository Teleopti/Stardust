(function() {
	'use strict';

	angular.module('wfm.teamSchedule').factory('PersonScheduleWeekViewCreator', PersonScheduleWeekViewCreator);
	PersonScheduleWeekViewCreator.$inject = ['$filter', 'CurrentUserInfo', 'colorUtils'];

	function PersonScheduleWeekViewCreator($filter, currentUserInfo, colorUtils) {
		function createPersonWeekViewModel(personWeek) {
			var days = [];
			angular.forEach(personWeek.DaySchedules, function(day) {
				var dayViewModel = {
					date: day.Date,
					summeryTitle: day.Title,
					summeryTimeSpan: getTimeSpanForAgentScheduleDay(day.DateTimeSpan, day.Timezone, day.Date.Date),
					isDayOff: day.IsDayOff,
					isTerminated: day.IsTerminated,
					color: day.Color,
					contrastFontColor: colorUtils.getTextColorBasedOnBackgroundColor(day.Color)
				};
				days.push(dayViewModel);
			});
			var personWeekViewModel = {
				personId: personWeek.PersonId,
				name: personWeek.Name,
				days: days,
				firstDayOfWeek: days[0].date.Date.split('T')[0],
				contractTime: formatContractTimeMinutes(personWeek.ContractTimeMinutes)
			}
			return personWeekViewModel;
		}

		function getTimeSpanForAgentScheduleDay(dateTimeSpan, timezone, dateInWeek) {
			if (!dateTimeSpan || !timezone || !dateInWeek) return '';

			var dateTimeFormat = currentUserInfo.CurrentUserInfo().DateTimeFormat || {};
			var shortTimePattern = dateTimeFormat.ShortTimePattern;
			
			var startTimeInUserTimezoneMoment = moment($filter('timezone')(dateTimeSpan.StartDateTime, null, timezone.IanaId));
			var endTimeInUserTimezoneMoment = moment($filter('timezone')(dateTimeSpan.EndDateTime, null, timezone.IanaId));

			var displayStarStr = startTimeInUserTimezoneMoment.format(shortTimePattern);
			var displayEndStr = endTimeInUserTimezoneMoment.format(shortTimePattern);

			if (startTimeInUserTimezoneMoment.isBefore(moment(dateInWeek).startOf('day'))) {
				displayStarStr = displayStarStr + ' (-1)';
			}else if(startTimeInUserTimezoneMoment.isAfter(moment(dateInWeek).endOf('day'))){
				displayStarStr = displayStarStr + ' (+1)';
			}

			if(endTimeInUserTimezoneMoment.isBefore(moment(dateInWeek).startOf('day'))) {
				displayEndStr = displayEndStr + ' (-1)';
			}else if (endTimeInUserTimezoneMoment.isAfter(moment(dateInWeek).endOf('day'))) {
				displayEndStr = displayEndStr + ' (+1)';
			}

			return displayStarStr + ' - ' + displayEndStr;
		}

		function formatContractTimeMinutes(minutes) {
			return Math.floor(minutes / 60) + ':' + (minutes % 60 === 0 ? '00' : minutes % 60);
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
})();