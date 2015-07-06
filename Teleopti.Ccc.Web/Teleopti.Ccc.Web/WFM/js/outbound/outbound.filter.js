(function() {

	'use strict';

	angular.module('wfm.outbound')
		.filter('showWeekdays', showWeekdays)
		.filter('showTimespan', showTimespan);


	function showWeekdays() {
		return function (input) {
			var weekdays = [
				"Sun",
				"Mon",
				"Tue",
				"Wed",
				"Thu",
				"Fri",
				"Sat"
			];
			if (input.WeekDay >= weekdays.length) {
				return "";
			}
			if (input < 0)
				return weekdays;
			return weekdays[input.WeekDay];
		};
	}

	function showTimespan() {
		return function (input) {
			if (input === null) return ' -- : -- ';

			var hours = Math.floor(input);
			var minutes = Math.ceil((input - hours) * 60);
			var minuteString = ('00' + minutes.toString()).slice(-2);
			return hours + ' : ' + minuteString;
		}
	}


})();