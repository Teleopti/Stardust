(function() {
	'use strict';
	angular.module("wfm.teamSchedule").service("UtilityService", utilityService);

	utilityService.$inject = ['CurrentUserInfo'];

	function utilityService(CurrentUserInfo) {

		var self = this;
		var tick = 15;

		self.getWeekdayNames = getWeekdayNames;
		self.getWeekdays = getWeekDays;
		self.getNextTick = getNextTick;
		self.getNextTickNoEarlierThanEight = getNextTickNoEarlierThanEight;
		self.nowInUserTimeZone = nowInUserTimeZone;
		self.setNowDate = setNowDate;
		self.now = now;


		var nowDate = new Date();

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