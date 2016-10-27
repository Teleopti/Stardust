(function() {
	angular.module("wfm.teamSchedule").service("UtilityService", utilityService);

	utilityService.$inject = ['$locale', 'WFMDate'];

	function utilityService($locale, WFMDate) {

		var self = this;
		var tick = 15;

		self.getWeekdayNames = getWeekdayNames;
		self.getWeekdays = getWeekDays;
		self.getNextTick = getNextTick;
		self.getNextTickNoEarlierThanEight = getNextTickNoEarlierThanEight;
		self.nowInUserTimeZone = WFMDate.nowInUserTimeZone;
			
		function getWeekdayNames() {
			var names = $locale.DATETIME_FORMATS.DAY;
			var defaultIdx = [6, 0, 1, 2, 3, 4, 5];
			var fdow = $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK;

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
			var nowInUserTimeZoneMoment = moment(WFMDate.nowInUserTimeZone());

			var minutes = Math.ceil(nowInUserTimeZoneMoment.minute() / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);
			return start.format();
		}

		function getNextTickNoEarlierThanEight() {
			var nowInUserTimeZoneMoment = moment(WFMDate.nowInUserTimeZone());

			var minutes = Math.ceil(nowInUserTimeZoneMoment.minute() / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);

			start.hours() < 8 && start.hours(8) && start.minutes(minutes);

			return start.format();
		}
	}
})();