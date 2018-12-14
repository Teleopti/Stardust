(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaFormatService', rtaFormatService);

	function rtaFormatService() {
		var service = {
			formatDateTime: formatDateTime,
			formatDuration: formatDuration,
			formatToSeconds: formatToSeconds,
			formatHexToRgb: formatHexToRgb,
			numberToPercent: numberToPercent,
			timeToPercent: timeToPercent,
			secondsToPercent: secondsToPercent,
			timePeriodToPercent: timePeriodToPercent,
			buildTimeline: buildTimeline
		}

		return service;
		//////////////////////////

		function formatDateTime(time) {
			if (time === null || angular.isUndefined(time) || time === '') return '';
			var momentTime = moment.utc(time);
			if (momentTime.format("YYYYMMDD") > moment().format("YYYYMMDD")) {
				return momentTime.format('YYYY-MM-DD HH:mm:ss');
			}
			return momentTime.format('HH:mm');
		};

		function formatDuration(seconds) {
			if (seconds === null) {
				return "";
			}
			var duration = moment.duration(seconds, 'seconds');
			return Math.floor(duration.asHours()) + moment.utc(seconds*1000).format(":mm:ss");
		};

		function formatToSeconds(duration) {
			return moment.duration(duration, 'seconds');
		}

		function formatHexToRgb(hex) {
			hex = hex ? hex.substring(1) : 'ffffff';
			var bigint = parseInt(hex, 16);
			var r = (bigint >> 16) & 255;
			var g = (bigint >> 8) & 255;
			var b = bigint & 255;
			return "rgba(" + r + ", " + g + ", " + b + ", 0.6)";
		};

		function numberToPercent(num1, num2) {
			return (Math.floor((num1 / num2) * 100));
		};

		function secondsToPercent(seconds) {
			return seconds / 3600 * 25;
		}

		function timeToPercent(currentTime, time) {
			var offset = moment(currentTime).add(-1, 'hour');
			return secondsToPercent(moment(time).diff(offset, 'seconds'));
		}

		function timePeriodToPercent(windowStart, startTime, endTime) {
			var start = moment(startTime) > windowStart ? moment(startTime) : windowStart;
			var lengthSeconds = moment(endTime).diff(start, 'seconds');
			return secondsToPercent(lengthSeconds);
		}

		function buildTimeline(statesTime) {
			var timeline = function (time) {
				var percent = timeToPercent(statesTime, time);
				return {
					Time: time.format('LT'),
					Offset: percent + "%"
				};
			};

			var time = moment(statesTime).startOf('hour');
			return [
				timeline(time),
				timeline(time.add(1, 'hour')),
				timeline(time.add(1, 'hour')),
				timeline(time.add(1, 'hour'))
			].filter(function (tl) {
				return tl != null;
			});
		}
	};
})();
