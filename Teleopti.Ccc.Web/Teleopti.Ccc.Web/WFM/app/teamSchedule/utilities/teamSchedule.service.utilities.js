﻿
(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("UtilityService", utilityService);

	utilityService.$inject = ['CurrentUserInfo', 'serviceDateFormatHelper'];

	function utilityService(CurrentUserInfo, serviceDateFormatHelper) {

		var self = this;
		var tick = 15;
		var currentUserInfo = CurrentUserInfo.CurrentUserInfo();

		self.getWeekdayNames = getWeekdayNames;
		self.getWeekdays = getWeekDays;
		self.getNextTickNoEarlierThanEight = getNextTickNoEarlierThanEight;
		self.nowInUserTimeZone = formattedNowInUserTimeZone;
		self.nowInSelectedTimeZone = formattedNowInSelectedTimeZone;
		self.setNowDate = setNowDate;
		self.now = now;
		self.getFirstDayOfWeek = getFirstDayOfWeek;

		var fakeNowDate;

		function setNowDate(date) {
			fakeNowDate = date;
		}

		function now() {
			if (fakeNowDate) return fakeNowDate;
			return new Date();
		};

		function nowMoment() {
			return moment(self.now());
		}

		function formattedNowInUserTimeZone() {
			return format(nowInUserTimeZone());
		}

		function formattedNowInSelectedTimeZone(timezone) {
			return format(nowInSelectedTimeZone(timezone));
		}

		function nowInUserTimeZone() {
			return nowMoment().clone().tz(currentUserInfo.DefaultTimeZone);
		}

		function nowInSelectedTimeZone(timezone) {
			return nowMoment().clone().tz(timezone);
		}


		function getWeekdayNames() {
			var names = currentUserInfo.DayNames || [];
			var defaultIdx = [0, 1, 2, 3, 4, 5, 6];
			var fdow = currentUserInfo.FirstDayOfWeek;
			var result = [];
			var startIndex = defaultIdx.indexOf(fdow);
			for (var i = 0; i < 7; i++) {
				result.push(names[(startIndex + i) % 7]);
			}
			return result;
		}

		function getFirstDayOfWeek(date) {
			var momentCopy = [moment].slice(0)[0];
			momentCopy.updateLocale(currentUserInfo.DateFormatLocale, {
				week: { dow: currentUserInfo.FirstDayOfWeek }
			});
			return serviceDateFormatHelper.getDateOnly(momentCopy(date).startOf('week'));
		}

		function getWeekDays(date) {
			var names = getWeekdayNames();
			var startOfWeek = moment(getFirstDayOfWeek(date));

			var dates = [];
			for (var i = 0; i < 7; i++) {
				dates.push({
					name: names[i],
					date: startOfWeek.clone().add(i, 'days').toDate()
				});
			}
			return dates;
		}

		function getNextTickNoEarlierThanEight(timezone) {
			var nowInUserTimeZoneMoment = nowInSelectedTimeZone(timezone);

			var minutes = Math.ceil((nowInUserTimeZoneMoment.minute() + 1) / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);

			if (start.hours() < 8)
				start.hours(8).minutes(0);

			return format(start);
		}

		function format(dateMoment) {

			return serviceDateFormatHelper.getDateByFormat(dateMoment, 'YYYY-MM-DDTHH:mm:ssZ');
		}
	}
})();