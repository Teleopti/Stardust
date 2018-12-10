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

	timeRangePickerCtrl.$inject = ['$element', '$attrs', '$timeout', 'serviceDateFormatHelper'];

	function timeRangePickerCtrl($element, $attrs, $timeout, serviceDateFormatHelper) {
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
			if (endEarlierThanStartOnTimeOnly()) {
				ctrl.isNextDay = false;
				ctrl.startDate = ctrl.date;
				ctrl.endDate = getNextDate();
			}
			else {
				setDateRange();
			}

			//$timeout(function () {
			ctrl.timeRange = angular.copy(ctrl.timeRange);
			ctrl.ngModelCtrl.$setViewValue(ctrl.timeRange);
			//});
		}

		ctrl.disableNextDay = endEarlierThanStartOnTimeOnly;

		ctrl.onIsNextDayChanged = setDateRange;

		function setDateRange() {
			if (!isValidTimeRange())
				return;
			var nextDate = getNextDate();
			var startDate = serviceDateFormatHelper.getDateOnly(ctrl.timeRange.startTime);
			var endDate = serviceDateFormatHelper.getDateOnly(ctrl.timeRange.endTime);

			if (ctrl.isNextDay) {
				if (startDate !== nextDate) ctrl.startDate = nextDate;
				if (endDate !== nextDate) ctrl.endDate = nextDate;
			} else {
				if (startDate !== ctrl.date) ctrl.startDate = ctrl.date;
				if (endDate !== ctrl.date) ctrl.endDate = ctrl.date;
			}
		}

		function isValidTimeRange() {
			return !!ctrl.timeRange.startTime && !!ctrl.timeRange.endTime;
		}

		function endEarlierThanStartOnTimeOnly() {
			return isValidTimeRange() && moment('1900-01-01 ' + getTimeOnlyInTimeZone(ctrl.timeRange.endTime))
				.isSameOrBefore('1900-01-01 ' + getTimeOnlyInTimeZone(ctrl.timeRange.startTime));
		}
		function getTimeOnlyInTimeZone(dateTime) {
			return serviceDateFormatHelper.getTimeOnly(moment.tz(dateTime, ctrl.timezone));
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