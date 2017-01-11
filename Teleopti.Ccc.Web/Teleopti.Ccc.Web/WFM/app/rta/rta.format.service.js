(function() {
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
			numberToPercent: numberToPercent
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

		function formatDuration(duration) {
			if (duration === null) {
				return "";
			}
			var durationInSeconds = moment.duration(duration, 'seconds');
			return (Math.floor(durationInSeconds.asHours()) + moment(durationInSeconds.asMilliseconds()).format(':mm:ss'));
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
	};
})();
