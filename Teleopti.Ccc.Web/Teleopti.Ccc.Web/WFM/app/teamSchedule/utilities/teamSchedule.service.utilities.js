
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
		self.getNextTickMomentNoEarlierThanEight = getNextTickMomentNoEarlierThanEight;
		self.nowInTimeZone = formattedNowInTimeZone;
		self.setNowDate = setNowDate;
		self.getFirstDayOfWeek = getFirstDayOfWeek;
		self.nowDateInUserTimezone = nowDateInUserTimezone;

		var fakeNowDate;

		function setNowDate(date) {
			fakeNowDate = date;
		}

		function now() {
			if (fakeNowDate) return fakeNowDate;
			return currentUserInfo.NowInUtc();
		};

		function nowMoment() {
			return moment.tz(now(), 'etc/UTC');
		}

		function formattedNowInTimeZone(timezone) {
			return format(nowInTimeZone(timezone));
		}


		function nowInTimeZone(timezone) {
			return nowMoment().clone().tz(timezone);
		}

		function nowDateInUserTimezone() {
			return serviceDateFormatHelper.getDateOnly(nowMoment().clone().tz(currentUserInfo.DefaultTimeZone));
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
			return format(getNextTickMomentNoEarlierThanEight(timezone));
		}

		function getNextTickMomentNoEarlierThanEight(timezone) {
			var nowInUserTimeZoneMoment = nowInTimeZone(timezone);

			var minutes = Math.ceil((nowInUserTimeZoneMoment.minute() + 1) / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);

			if (start.hours() < 8)
				start.hours(8).minutes(0);

			return start;
		}

		function format(dateMoment) {
			return serviceDateFormatHelper.getDateByFormat(dateMoment, 'YYYY-MM-DDTHH:mm:ssZ');
		}
	}
})();