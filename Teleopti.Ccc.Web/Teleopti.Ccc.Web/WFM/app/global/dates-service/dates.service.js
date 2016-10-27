(function() {
	'use strict';

	angular
		.module('wfmDate', ['currentUserInfoService'])
		.factory('WFMDate', WFMDate);

	WFMDate.$inject = ['CurrentUserInfo'];

	function WFMDate(CurrentUserInfo) {

		var nowDate = new Date();
		
		var service = {
			setNowDate: setNowDate,
			now: now,
			nowMoment: nowMoment,
			nowInUserTimeZone: nowInUserTimeZone		
		};

		return service;


		function setNowDate(date) {
			nowDate = date;
		}

		function now() {
			return nowDate;
		};

		function nowMoment() {
			return moment(nowDate);
		}

		function nowInUserTimeZone() {
			return moment.tz(nowMoment(), CurrentUserInfo.DefaultTimeZone).format();
		}
	}
})();
