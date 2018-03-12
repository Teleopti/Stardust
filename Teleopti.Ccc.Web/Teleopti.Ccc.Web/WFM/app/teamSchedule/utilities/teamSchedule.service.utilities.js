(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("UtilityService", utilityService);

	utilityService.$inject = ['CurrentUserInfo', 'serviceDateFormatHelper'];

	function utilityService(CurrentUserInfo, serviceDateFormatHelper) {

		var self = this;
		var tick = 15;

		self.getWeekdayNames = getWeekdayNames;
		self.getWeekdays = getWeekDays;
		self.getNextTickNoEarlierThanEight = getNextTickNoEarlierThanEight;
		self.nowInUserTimeZone = formattedNowInUserTimeZone;
		self.nowInSelectedTimeZone = formattedNowInSelectedTimeZone;
		self.setNowDate = setNowDate;
		self.now = now;

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
			return nowMoment().clone().tz(CurrentUserInfo.CurrentUserInfo().DefaultTimeZone);
		}

		function nowInSelectedTimeZone(timezone) {
			return nowMoment().clone().tz(timezone);
		}


		function getWeekdayNames() {
			var localeData = moment.localeData();
			var names = localeData.weekdays();
			var defaultIdx = [0, 1, 2, 3, 4, 5, 6];
			var fdow = localeData.firstDayOfWeek();
			var result = [];
			var startIndex = defaultIdx.indexOf(fdow);
			for (var i = 0; i < 7; i++) {
				result.push(names[(startIndex + i) % 7]);
			}
			return result;
		}

		function getWeekDays(date) {
			var names = getWeekdayNames();
			var startOfWeek = moment(date).startOf('week');

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
			var nowInUserTimeZoneMoment = moment(nowInSelectedTimeZone(timezone).format('YYYY-MM-DD HH:mm'));

			var minutes = Math.ceil((nowInUserTimeZoneMoment.minute() + 1) / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);

			start.hours() < 8 && start.hours(8) && start.minutes(minutes);

			return format(moment.tz(start.format('YYYY-MM-DD HH:mm'),timezone));
		}

		function format(dateMoment) {
			
			return serviceDateFormatHelper.getDateByFormat(dateMoment, 'YYYY-MM-DDTHH:mm:ssZ');
		}
	}
})();