(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.component('teamsTimePicker',
		{
			templateUrl: 'app/teamSchedule/html/teamsTimePicker.tpl.html',
			require: {
				ngModelCtrl: 'ngModel'
			},
			bindings: {
				ngModel: "=?",
				date: '<?',
				timezone: '<?',
				minuteStep: '<?'
			},
			controller: timePickerCtrl
		});

	timePickerCtrl.$inject = ['$scope', '$element', '$attrs', '$locale', 'serviceDateFormatHelper'];

	function timePickerCtrl($scope, $element, $attrs, $locale, serviceDateFormatHelper) {
		var ctrl = this;

		ctrl.dateTimeObj = typeof (ctrl.ngModel) === 'string' ?
			moment('1900-01-01 ' + serviceDateFormatHelper.getTimeOnly(moment.tz(ctrl.ngModel, 'UTC'))).toDate() : undefined;

		var meridianInfo = getMeridiemInfoFromMoment($locale);
		ctrl.showMeridian = meridianInfo.showMeridian;
		ctrl.meridians = ctrl.showMeridian ? [meridianInfo.am, meridianInfo.pm] : [];


		ctrl.$onChanges = function () {
			ctrl.onTimeChange();
		}

		ctrl.$onInit = function () {
			ctrl.minuteStep = ctrl.minuteStep || 1;
		}

		ctrl.onTimeChange = function () {
			ctrl.ngModelCtrl.$setValidity('dst', !!getValidDateTimeInTimezone(ctrl.dateTimeObj));
			ctrl.ngModelCtrl.$setViewValue(getDateTime(ctrl.dateTimeObj));
		}

		function getValidDateTimeInTimezone(dateObj) {
			var dateTime = getDateTime(dateObj);
			if (!dateTime)
				return null;
			var dateTimeInTimeZone = serviceDateFormatHelper.getDateTime(moment.tz(dateTime, ctrl.timezone));
			if (dateTimeInTimeZone === dateTime)
				return dateTimeInTimeZone;
			return null;
		}

		function getDateTime(dateObj) {
			if (!dateObj)
				return null;
			var time = serviceDateFormatHelper.getTimeOnly(dateObj);
			return serviceDateFormatHelper.getDateOnly(ctrl.date) + ' ' + time;
		}

		function getMeridiemInfoFromMoment($locale) {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			var info = {};

			if (/h:/.test(timeFormat)) {
				info.showMeridian = true;
				info.am = $locale.DATETIME_FORMATS.AMPMS[0];
				info.pm = $locale.DATETIME_FORMATS.AMPMS[1];
			} else {
				info.showMeridian = false;
			}
			return info;
		}
	}
})();