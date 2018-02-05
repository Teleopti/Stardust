(function () {
	'use strict';

	angular.module('wfm.rta')
		.service('RtaTimeline', function () {

			function positionForTime(offsetTime, time, timelineTotalSeconds) {
				var diff = moment(time).diff(moment(offsetTime), 'seconds');
				return (diff / timelineTotalSeconds) * 100 + '%';
			}

			function makeOffsetCalculator(timelineStart, timelineEnd) {
				return function (time) {
					if (moment(time).isBefore(timelineStart))
						return '0%';
					var timelineTotalSeconds = timelineEnd.diff(timelineStart, 'seconds');
					return positionForTime(timelineStart, time, timelineTotalSeconds);
				}
			}

			function makeWidthCalculator(timelineStart, timelineEnd) {
				return function (start, end) {
					if (moment(start).isBefore(timelineStart))
						start = timelineStart;
					var timelineTotalSeconds = timelineEnd.diff(timelineStart, 'seconds');
					return positionForTime(start, end, timelineTotalSeconds);
				}
			}

			return {
				makeCalculator: function (timelineStart, timelineEnd) {
					timelineStart = moment(timelineStart);
					timelineEnd = moment(timelineEnd);
					return {
						Offset: makeOffsetCalculator(timelineStart, timelineEnd),
						Width: makeWidthCalculator(timelineStart, timelineEnd),
					}
				}
			}
		});

})();
