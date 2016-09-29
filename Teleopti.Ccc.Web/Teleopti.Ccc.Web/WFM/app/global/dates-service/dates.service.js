(function() {
	'use strict';

	angular
		.module('wfmDate', ['currentUserInfoService'])
		.factory('WFMDate', WFMDate);

	WFMDate.$inject = ['CurrentUserInfo'];

	function WFMDate(CurrentUserInfo) {

		var nowDate = new Date();
		var tick = 15;

		var service = {
			setNowDate: setNowDate,
			now: now,
			nowMoment: nowMoment,
			nowInUserTimeZone: nowInUserTimeZone,
			getNextTick: getNextTick,
			getNextTickNoEarlierThanEight: getNextTickNoEarlierThanEight
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

		function getNextTick() {
			var nowInUserTimeZoneMoment = moment(nowInUserTimeZone());

			var minutes = Math.ceil(nowInUserTimeZoneMoment.minute() / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);
			return start.format();
		}

		function getNextTickNoEarlierThanEight() {
			var nowInUserTimeZoneMoment = moment(nowInUserTimeZone());

			var minutes = Math.ceil(nowInUserTimeZoneMoment.minute() / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);

			start.hours() < 8 && start.hours(8) && start.minutes(minutes);

			return start.format();
		}


	}
})();
