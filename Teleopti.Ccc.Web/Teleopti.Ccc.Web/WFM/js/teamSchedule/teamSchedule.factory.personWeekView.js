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
					isTerminated: day.IsTerminated,
					color: day.Color,
					contrastFontColor: getContrastColor(day.Color)
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

		function rgbToHexColor(rgbcolor){
			var rgb =  rgbcolor.match(/^rgba?[\s+]?\([\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?/i);

			return (rgb && rgb.length === 4) ? "#" +
				("0" + parseInt(rgb[1], 10).toString(16)).slice(-2) +
				("0" + parseInt(rgb[2], 10).toString(16)).slice(-2) +
				("0" + parseInt(rgb[3], 10).toString(16)).slice(-2) : '';
		}

		function getContrastColor(rgbcolor) {
			if(!rgbcolor) return 'black';

			var hexcolor = rgbToHexColor(rgbcolor);

			if (hexcolor[0] === '#') {
				hexcolor = hexcolor.substring(1);
			}

			var r = parseInt(hexcolor.substr(0, 2), 16);
			var g = parseInt(hexcolor.substr(2, 2), 16);
			var b = parseInt(hexcolor.substr(4, 2), 16);
			var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;

			return (yiq >= 128) ? 'black' : 'white';
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