(function() {
	'use strict';

	angular.module('wfm.outbound')
		.directive('timepickerWrap', timepickerWrapDirective);

	function timepickerWrapDirective() {

		var meridianInfo = getMeridiemInfoFromMoment();

		return {
			template: '<timepicker></timepicker>',
			controller: ['$scope', timepickerWrapCtrl],
			compile: compileFn
		};

		function compileFn(tElement, tAttributes) {
			var binding = tAttributes.ngModel;
			tElement.addClass('wfm-timepicker-wrap');

			var cellElement = tElement.find('timepicker');
			cellElement.attr('ng-model', binding);
			cellElement.attr('show-meridian', 'showMeridian');
			cellElement.attr('minute-step', 'minuteStep');

			if (meridianInfo.showMeridian) {
				cellElement.attr('meridians', 'meridians');
			}
		}

		function timepickerWrapCtrl($scope) {
			$scope.showMeridian = meridianInfo.showMeridian;
			$scope.minuteStep = 5;

			if (meridianInfo.showMeridian) {
				$scope.meridians = [meridianInfo.am, meridianInfo.pm];
			}
		}
	}

	function getMeridiemInfoFromMoment() {
		var timeFormat = moment.localeData()._longDateFormat.LT;
		var info = {};

		if (/h/.test(timeFormat)) {
			info.showMeridian = true;
			info.am = moment.localeData().meridiem(9, 0);
			info.pm = moment.localeData().meridiem(15, 0);
		} else {
			info.showMeridian = false;
		}
		return info;
	}

})();