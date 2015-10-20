'use strict';

(function () {

	angular.module('wfm.teamSchedule').factory('shiftHelper', shiftHelper);

	shiftHelper.$inject = [];

	function shiftHelper() {
		var service = {};

		service.GetTextBackgroundColor = function(backgroundColor) {
			backgroundColor = backgroundColor.slice(backgroundColor.indexOf('(') + 1, backgroundColor.indexOf(')'));

			var backgroundColorArr = backgroundColor.split(',');

			var brightness = backgroundColorArr[0] * 0.299 + backgroundColorArr[1] * 0.587 + backgroundColorArr[2] * 0.114;

			return brightness < 100 ? 'rgb(255,255,255)' : 'rgb(0,0,0)';
		}
		service.HexToRgb = function(hex) {
			var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
			var rgb = result ? {
				r: parseInt(result[1], 16),
				g: parseInt(result[2], 16),
				b: parseInt(result[3], 16)
			} : null;
			if (rgb)
				return "rgb(" + rgb.r + "," + rgb.g + "," + rgb.b + ")";
			return rgb;
		}

		return service;
	};

}());