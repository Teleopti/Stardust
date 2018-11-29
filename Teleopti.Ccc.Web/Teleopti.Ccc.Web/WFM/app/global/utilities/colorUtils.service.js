(function() {
	'use strict';

	angular.module('wfm.utilities').service('colorUtils', colorUtils);

	function colorUtils() {
		//Note: for the unified look of the activity color and text in Web,
		//please keep this color strategy synced with the one in MyTime: Teleopti.MyTimeWeb.Common.js
		//which locates in \Teleopti.Ccc.Web\Teleopti.Ccc.Web\Areas\MyTime\Content\Scripts\Teleopti.MyTimeWeb.Common.js
		var svc = this;

		svc.getTextColorBasedOnBackgroundColor = function(backgroundColor) {
			if (typeof backgroundColor != 'string' || backgroundColor.length == 0) return 'black';

			if (backgroundColor.indexOf('#') > -1) {
				backgroundColor = svc.hexToRGB(backgroundColor);
			}

			backgroundColor = backgroundColor.slice(backgroundColor.indexOf('(') + 1, backgroundColor.indexOf(')'));

			var backgroundColorArr = backgroundColor.split(',');

			var brightness =
				backgroundColorArr[0] * 0.299 + backgroundColorArr[1] * 0.587 + backgroundColorArr[2] * 0.114;

			return brightness < 128 ? 'white' : 'black';
		};

		svc.hexToRGB = function(hex) {
			var result = /^#?([a-f\d]{2}|[a-f\d]{1})([a-f\d]{2}|[a-f\d]{1})([a-f\d]{2}|[a-f\d]{1})$/i.exec(hex);
			var rgb = result
				? {
						r: parseInt(fillupDigits(result[1]), 16),
						g: parseInt(fillupDigits(result[2]), 16),
						b: parseInt(fillupDigits(result[3]), 16)
				  }
				: null;
			if (rgb) return 'rgb(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ')';
			return rgb;
		};

		svc.colorToRGB = function(color) {
			if (!color) {
				return '';
			}
			if (color.indexOf('#') > -1) {
				return color;
			} else if (color.split(',').length == 3) {
				return 'rgb(' + color + ')';
			} else {
				return 'rgba(' + color + ')';
			}
		};

		function fillupDigits(hex) {
			if (hex.length == 1) {
				hex += '' + hex;
			}
			return hex;
		}
	}
})();
