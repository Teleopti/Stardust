﻿(function () {
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
				availableTimezones: '<',
				ngModel: '=?'
			}
		});

	timezonePickerCtrl.$inject = ['$timeout', 'CurrentUserInfo'];

	function timezonePickerCtrl($timeout, currentUserInfo) {
		var ctrl = this;
		var defaultTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
		var defaultTimezoneName = currentUserInfo.CurrentUserInfo().DefaultTimeZoneName;
		ctrl.ngModel = ctrl.ngModel || defaultTimezone;
		ctrl.selectedTimezone = ctrl.ngModel;

		ctrl.$onChanges = function (changesObj) {
			if (!changesObj.availableTimezones) return;

			populateTimezoneList();

			if (!!ctrl.timezoneList.length && !isInTimezoneList(ctrl.selectedTimezone)) {
				ctrl.selectedTimezone = defaultTimezone;
				$timeout(function () {
					ctrl.onSelectionChanged();
				});
			}

			unshiftDefaultTimezone();
		}

		ctrl.isSelectedEnabled = function (timezoneId) {
			return timezoneId && (timezoneId.indexOf('invalidIanaId') < 0);
		}

		ctrl.onSelectionChanged = function () {
			ctrl.ngModel = ctrl.selectedTimezone;
			ctrl.ngModelCtrl.$setViewValue(ctrl.ngModel);
		}

		ctrl.shortDisplayNameOfTheSelected = function () {
			var reg = /\((.+?)\)/;
			var displayName = '';
			for (var i = 0; i < ctrl.timezoneList.length; i++) {
				if (ctrl.timezoneList[i].ianaId === ctrl.selectedTimezone) {
					displayName = ctrl.timezoneList[i].displayName;
					break;
				}
			}
			var result = reg.exec(displayName);
			return result ? result[1] : displayName;
		}

		function populateTimezoneList() {
			var timezoneDict = {};
			ctrl.availableTimezones.forEach(function (z) {
				if (z.IanaId === "") {
					timezoneDict['invalidIanaId' + ' ' + z.DisplayName] = z.DisplayName;
				} else {
					timezoneDict[z.IanaId] = z.DisplayName;
				}

			});
			ctrl.timezoneList = [];

			angular.forEach(timezoneDict, function (value, key) {
				ctrl.timezoneList.push({
					ianaId: key,
					displayName: value
				});
			});
		}

		function unshiftDefaultTimezone() {
			if (!isInTimezoneList(defaultTimezone))
				ctrl.timezoneList.unshift({
					ianaId: defaultTimezone,
					displayName: defaultTimezoneName
				});
		}

		function isInTimezoneList(timezone) {
			return !!ctrl.timezoneList.filter(function (tz) { return tz.ianaId == timezone; }).length;
		}

	}

})();