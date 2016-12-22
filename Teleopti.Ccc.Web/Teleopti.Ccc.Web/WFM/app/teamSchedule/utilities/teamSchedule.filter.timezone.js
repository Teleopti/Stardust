(function() {

	angular.module('wfm.teamSchedule').filter('timezone', timezoneFilter);

	timezoneFilter.$inject = ['CurrentUserInfo'];
	function timezoneFilter(currentUserInfo) {		
		return function (inputString, toTimezone, fromTimezone) {
			
			if (!fromTimezone) fromTimezone =  currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			if (!toTimezone) toTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			var dateString;
			var timeOnly = false;
			
			if (/^\s*\d{1,2}:\d{2,2}/.test(inputString)) {
				dateString = moment().format("YYYY-MM-DD") + " " + inputString;
				timeOnly = true;
			} else {
				dateString = inputString;
			}

			var converted = moment.tz(dateString, fromTimezone).clone().tz(toTimezone);
			return converted.format(timeOnly ? "HH:mm" : "YYYY-MM-DDTHH:mm");
		}
	}

})();