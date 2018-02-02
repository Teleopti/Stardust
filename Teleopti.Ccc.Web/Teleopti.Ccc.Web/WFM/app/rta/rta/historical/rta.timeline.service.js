(function () {
	'use strict';
	
	angular.module('wfm.rta')
		.service('RtaTimeline', function () {

			function positionForTime(offsetTime, time, totalSeconds) {
				var diff = moment(time).diff(moment(offsetTime), 'seconds');
				return (diff / totalSeconds) * 100 + '%';
			}

			function makeOffsetCalculator(offsetTime, totalSeconds) {
				return function (time) {
					if (moment(time).isBefore(offsetTime))
						return '0%';
					return positionForTime(offsetTime, time, totalSeconds);
				}
			}

			function makeWidthCalculator(offsetTime, totalSeconds) {
				return function (start, end) {
					if (moment(start).isBefore(offsetTime))
						start = offsetTime;
					return positionForTime(start, end, totalSeconds);
				}
			}

			return {
				makeCalculator: function (timelineStart, timelineEnd) {
					timelineStart = moment(timelineStart);
					timelineEnd = moment(timelineEnd);
					var totalSeconds = timelineEnd.diff(timelineStart, 'seconds');

					return {
						Offset: makeOffsetCalculator(timelineStart, totalSeconds),
						Width: makeWidthCalculator(timelineStart, totalSeconds),
					}
				}
			}
		});

})();
