(function() {
	angular.module("wfm.teamSchedule").service("UtilityService", UtilityService);

	UtilityService.$inject = ['$locale'];

	function UtilityService($locale) {

		var self = this;

		self.getWeekdayNames = getWeekdayNames;
		self.getWeekdays = getWeekDays;
			
		function getWeekdayNames() {
			var names = $locale.DATETIME_FORMATS.DAY;
			var defaultIdx = [6, 0, 1, 2, 3, 4, 5];
			var sdow = $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK;

			var result = [];
			var startIndex = defaultIdx.indexOf(sdow);
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
	}
})();