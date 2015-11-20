(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaFormatService',
		function() {
			this.formatDateTime = function(time) {
				if(time===null || time===undefined || time==='') return '';
				var momentTime = moment.utc(time);
				if (momentTime.format("YYYYMMDD") > moment().format("YYYYMMDD")) {
					return momentTime.format('YYYY-MM-DD HH:mm:ss');
				}
				return momentTime.format('HH:mm');
			};

			this.formatDuration = function(duration) {
				var durationInSeconds = moment.duration(duration, 'seconds');
				return (Math.floor(durationInSeconds.asHours()) + moment(durationInSeconds.asMilliseconds()).format(':mm:ss'));
			};

			this.formatHexToRgb = function(hex) {
				hex = hex ? hex.substring(1) : 'ffffff';
				var bigint = parseInt(hex, 16);
				var r = (bigint >> 16) & 255;
				var g = (bigint >> 8) & 255;
				var b = bigint & 255;
				return "rgba(" + r + ", " + g + ", " + b + ", 0.6)";
			};

			this.numberToPercent = function(num1, num2) {
				return (Math.floor((num1 / num2) * 100));
			};
		}
	);
})();
