(function () {
	'use strict';

	angular.module("wfm.teamSchedule")
		.component('timezonePicker',
		{
			templateUrl: 'app/teamSchedule/html/timezonePicker.tpl.html',
			controller: timezonePickerCtrl,
			require: {
				ngModelCtrl: 'ngModel'
			},
			bindings: {
				avaliableTimezones: '<',
				ngModel: '=?',
				isDisabled: '<'
			}
		});

	timezonePickerCtrl.$inject = ['$timeout', 'CurrentUserInfo', 'TimezoneListFactory'];

	function timezonePickerCtrl($timeout, currentUserInfo, TimezoneListFactory) {
		var ctrl = this;
		var defaultTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

		ctrl.ngModel = ctrl.ngModel || defaultTimezone;
		ctrl.selectedTimezone = ctrl.ngModel;
		ctrl.timezoneList = {};

		ctrl.$onChanges = function (changesObj) {
			if (!ctrl.avaliableTimezones) return;
			ctrl.avaliableTimezones = ctrl.avaliableTimezones || [];
			if (!!ctrl.avaliableTimezones.length && ctrl.avaliableTimezones.indexOf(ctrl.selectedTimezone) < 0) {
				ctrl.selectedTimezone = defaultTimezone;
				$timeout(function () {
					ctrl.onSelectionChanged();
				});
			}

			ctrl.avaliableTimezones.push(defaultTimezone);
			TimezoneListFactory.Create(ctrl.avaliableTimezones).then(function (timezoneList) {
				ctrl.timezoneList = timezoneList;
			});
		}


		ctrl.isSelectedEnabled = function (timezoneId) {
			return timezoneId && (timezoneId.indexOf('invalidIanaId') < 0);
		}

		ctrl.onSelectionChanged = function () {
			ctrl.ngModel = ctrl.selectedTimezone;
			ctrl.ngModelCtrl.$setViewValue(ctrl.ngModel);
			
		}
	}

})();