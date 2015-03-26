var outboundFilter = angular.isDefined(angular.module('wfm.outbound')) ?
	angular.module('wfm.outbound') : angular.module('wfm.outbound', []);


outboundFilter.filter('showWorkinghours', function () {
	return function (input) {
		var workingHours = [
			"07:00-08:00",
			"08:00-09:00",
			"09:00-10:00",
			"10:00-11:00",
			"11:00-12:00",
			"12:00-13:00",
			"13:00-14:00",
			"14:00-15:00",
			"15:00-16:00",
			"16:00-17:00",
			"17:00-18:00"
		];
		if (input >= workingHours.length) {
			return "";
		}
		if (input < 0)
			return workingHours;
		return workingHours[input];
	};
}).filter('showWeekdays', function() {
	return function(input) {
		var weekdays = [
			"Mon",
			"Tue",
			"Wed",
			"Thu",
			"Fri",
			"Sat",
			"Sun"
		];
		if (input >= weekdays.length) {
			return "";
		}
		if (input < 0)
			return weekdays;
		return weekdays[input];
	};
});

