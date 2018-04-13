(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.component('teamsTimePicker',
		{
			templateUrl: 'app/teamSchedule/html/teamsTimePicker.tpl.html',
			//require: {
			//	ngModelCtrl: "ngModel"
			//},
			bindings: {
				ngModel: "=?",
				date: '<?',
				timezone: '<?'
			},
			controller: timePickerCtrl
		});

	timePickerCtrl.$inject = ['$scope', '$element', '$attrs', '$locale', 'serviceDateFormatHelper'];

	function timePickerCtrl($scope, $element, $attrs, $locale, serviceDateFormatHelper) {
		var ctrl = this;

		ctrl.dateTimeObj = moment('1900-01-01 ' + serviceDateFormatHelper.getTimeOnly(ctrl.ngModel)).toDate();

		ctrl.$onChanges = function () {
			ctrl.onTimeChange();
		}

		var meridianInfo = getMeridiemInfoFromMoment($locale);

		ctrl.showMeridian = meridianInfo.showMeridian;
		ctrl.meridians = ctrl.showMeridian ? [meridianInfo.am, meridianInfo.pm] : [];
		ctrl.minuteStep = 5;

		ctrl.onTimeChange = function () {
			if (!ctrl.dateTimeObj) {
				ctrl.ngModel = null;
				return;
			}
			ctrl.ngModel = getValidDateTimeInTimezone(ctrl.dateTimeObj);
		}

		function getValidDateTimeInTimezone(dateObj) {
			if (!dateObj)
				return null;
			var timezone = ctrl.timezone;
			var time = serviceDateFormatHelper.getTimeOnly(dateObj);
			var dateTime = ctrl.date + ' ' + time;
			var dateTimeInTimeZone = serviceDateFormatHelper.getDateTime(moment.tz(dateTime, timezone));
			if (dateTimeInTimeZone === dateTime)
				return dateTimeInTimeZone;
			return null;
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