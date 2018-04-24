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
		ctrl.startDate = moment(ctrl.date).isSame(ctrl.timeRange.startTime, 'day') ? ctrl.date : serviceDateFormatHelper.getDateOnly(ctrl.timeRange.startTime);
		ctrl.endDate = moment(ctrl.date).isSame(ctrl.timeRange.endTime, 'day') ? ctrl.date : serviceDateFormatHelper.getDateOnly(ctrl.timeRange.endTime);


		ctrl.$onInit = function () {
			addTabindexToTimePicker();
			ctrl.isNextDay = serviceDateFormatHelper.getDateOnly(ctrl.timeRange.startTime) === getNextDate();
			ctrl.onTimeRangeChange();
		}

		ctrl.onTimeRangeChange = function () {
			var isValidTime = ctrl.timeRange.startTime && ctrl.timeRange.endTime;
			if (isValidTime) {
				if (endEarlierThanStartOnTimeOnly()) {
					ctrl.isNextDay = false;
					ctrl.timeRange.startTime = serviceDateFormatHelper.getDateOnly(ctrl.date) + ' ' + serviceDateFormatHelper.getTimeOnly(ctrl.timeRange.startTime);
					ctrl.timeRange.endTime = getNextDate() + ' ' + serviceDateFormatHelper.getTimeOnly(ctrl.timeRange.endTime);
				} else {
					ctrl.onIsNextDayChanged();
				}
			}
			
			ctrl.timeRange = angular.copy(ctrl.timeRange);
			ctrl.ngModelCtrl.$setViewValue(ctrl.timeRange);
		}

		ctrl.disableNextDay = endEarlierThanStartOnTimeOnly;
		

		ctrl.onIsNextDayChanged = function () {
			if (ctrl.isNextDay) {
				ctrl.startDate = getNextDate();
				ctrl.endDate = getNextDate();
			} else {
				ctrl.startDate = angular.copy(ctrl.date);
				ctrl.endDate = angular.copy(ctrl.date);
			}
		}

		function endEarlierThanStartOnTimeOnly() {
			return moment('1900-01-01 ' + serviceDateFormatHelper.getTimeOnly(ctrl.timeRange.endTime))
				.isSameOrBefore('1900-01-01 ' + serviceDateFormatHelper.getTimeOnly(ctrl.timeRange.startTime));
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