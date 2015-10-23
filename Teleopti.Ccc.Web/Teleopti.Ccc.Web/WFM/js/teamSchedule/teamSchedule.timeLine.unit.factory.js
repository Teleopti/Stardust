(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('TimeLineUnit', ['CurrentUserInfo', TimeLineUnit]);

	function TimeLineUnit(currentUserInfo) {
		//timeLineUnit.$inject = [];
		var timeLineUnit = {};
		var startTime;
		var startTimeMinutes;
		var lengthMinutes;

		timeLineUnit.init = function (projection, timeLine) {
			startTime = moment.tz(projection.Start, currentUserInfo.DefaultTimeZone);
			startTimeMinutes = startTime.diff(projection.Offset, 'minutes');
			lengthMinutes = projection.Minutes;
		}

		timeLineUnit.CutInsideDayStartMinutes = function() {
			return (startTimeMinutes >= 0) ? startTimeMinutes : 0;
		};

		return timeLineUnit;
	}
}());
