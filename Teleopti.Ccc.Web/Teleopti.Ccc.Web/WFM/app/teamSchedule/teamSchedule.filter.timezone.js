(function() {

	angular.module('wfm.teamSchedule').filter('timezone', timezoneFilter);

	timezoneFilter.$inject = ['CurrentUserInfo'];
	function timezoneFilter(currentUserInfo) {		
		return function(inputString, timezone) {
			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var dateString;
			var timeOnly = false;
			
			if (/^\s*\d{1,2}:\d{2,2}/.test(inputString)) {
				dateString = moment().format("YYYY-MM-DD") + " " + inputString;
				timeOnly = true;
			} else {
				dateString = inputString;
			}

			var converted = moment.tz(dateString, currentUserTimezone).tz(timezone);
			return converted.format(timeOnly ? "hh:mm" : "YYYY-MM-DDThh:mm");
		}
	}

})();