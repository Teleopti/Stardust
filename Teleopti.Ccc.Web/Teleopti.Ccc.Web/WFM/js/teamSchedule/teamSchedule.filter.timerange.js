(function() {

	angular.module('wfm.teamSchedule').filter('timespan', TimeSpanFilter);
	angular.module('wfm.teamSchedule').filter('timerange', TimeRangeFilter);

	function TimeRangeFilter() {
		return _transformTimeRange;
	}

	function TimeSpanFilter() {
		return _transformTimeSpanString;
	}

	function _transformTimeRange(timeRange) {
		if (!timeRange || !timeRange.StartTime || !timeRange.EndTime) return '';
		return _transformTimeSpanString(timeRange.StartTime) + ' - ' + _transformTimeSpanString(timeRange.EndTime);
	}


	function _transformTimeSpanString(str) {
		var regex = /(1\.)?(\d{2}):(\d{2}):(\d{2})/;
		var matchResult = regex.exec(str);
		if (!matchResult) return '';

		var hours = parseInt(matchResult[2]),
			minutes = parseInt(matchResult[3]),
			seconds = parseInt(matchResult[4]),
			days = matchResult[1] ? parseInt(matchResult[1]) : 0;

		var time = moment().startOf('day').add(hours, 'hours')
			.add(minutes, 'minutes').add(seconds, 'seconds');

		return days == 0 ? time.format('LT') : time.format('LT') + ' +' + days;
	}

})();