(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.component('teamsTimeRangePicker', {
			require: {
				ngModelCtrl: 'ngModel'
			},
			bindings: {
				ngModel: '=',
				date: '<',
				timezone: '<?',
				minuteStep: '<?'
			},
			controller: timeRangePickerCtrl,
			templateUrl: 'app/teamSchedule/html/teamsTimeRangePicker.tpl.html',
		});

	timeRangePickerCtrl.$inject = ['$element', '$attrs', '$locale', 'serviceDateFormatHelper'];

	function timeRangePickerCtrl($element, $attrs, $locale, serviceDateFormatHelper) {
		var ctrl = this;

		ctrl.timeRange = ctrl.ngModel;
		ctrl.startDate = ctrl.date;
		ctrl.endDate = ctrl.date;

		ctrl.$onInit = function () {
			addTabindexToTimePicker();
			ctrl.isNextDay = serviceDateFormatHelper.getDateOnly(ctrl.timeRange.startTime) === getNextDate();
			ctrl.onTimeRangeChange();
		}

		ctrl.ngModelChange = function () {
			ctrl.ngModelCtrl.$setViewValue(ctrl.ngModel);
		};

		ctrl.onTimeRangeChange = function () {
			if (isSameOrBefore()) {
				ctrl.isNextDay = false;
			}
		}

		ctrl.disableNextDay = isSameOrBefore;

		ctrl.onIsNextDayChanged = function () {
			if (ctrl.isNextDay) {
				ctrl.startDate = getNextDate();
				ctrl.endDate = getNextDate();
			} else if (isSameOrBefore()) {
				ctrl.startDate = ctrl.date;
				ctrl.endDate = getNextDate();
			} else {
				ctrl.startDate = ctrl.date;
				ctrl.endDate = ctrl.date;
			}
		}

		function isSameOrBefore() {
			return moment(ctrl.timeRange.endTime).isSameOrBefore(ctrl.timeRange.startTime);
		}

		function getNextDate() {
			return serviceDateFormatHelper.getDateOnly(moment(ctrl.date).add(1, 'days'));
		}

		function addTabindexToTimePicker() {
			var tabindex = angular.isDefined($attrs.tabindex) ? $attrs.tabindex : '0';
			angular.forEach($element[0].querySelectorAll('[uib-timepicker]'), function (timepicker) {
				timepicker.setAttribute('tabIndex', tabindex);
			});
		}
	}
})();