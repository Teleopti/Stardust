(function () {
	'use strict';

	angular.module('wfm.teamSchedule').filter('timezone', timezoneFilter);
	timezoneFilter.$inject = ['CurrentUserInfo'];
	function timezoneFilter(currentUserInfo) {
		return function (inputString, toTimezone, fromTimezone) {
			return convertToTimezone(currentUserInfo, inputString, toTimezone, fromTimezone);
		}
	}

	angular.module('wfm.teamSchedule').filter('serviceTimezone', serviceTimezoneFilter);
	serviceTimezoneFilter.$inject = ['CurrentUserInfo', 'serviceDateFormatHelper'];
	function serviceTimezoneFilter(currentUserInfo, serviceDateFormatHelper) {
		return function (inputString, toTimezone, fromTimezone) {
			return convertToTimezone(currentUserInfo, inputString, toTimezone, fromTimezone, serviceDateFormatHelper.getDateByFormat);
		}
	}

	function convertToTimezone(currentUserInfo, inputString, toTimezone, fromTimezone, getFormat) {
		if (!fromTimezone) fromTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
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
		var format = timeOnly ? "HH:mm" : "YYYY-MM-DDTHH:mm";
		return getFormat ? getFormat(converted, format) : converted.format(format);
	}

})();