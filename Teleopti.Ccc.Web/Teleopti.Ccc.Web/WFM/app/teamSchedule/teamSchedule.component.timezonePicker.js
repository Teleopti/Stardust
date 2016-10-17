(function () {
	'use strict';

	angular.module("wfm.teamSchedule")
		.component('timezonePicker',
		{
			templateUrl: 'app/teamSchedule/html/timezonePicker.tpl.html',
			controller: timezonePickerCtrl,
			bindings: {
				availableTimezones: '<',
				onPick: '&'
			}
		});

	timezonePickerCtrl.$inject = ['CurrentUserInfo'];

	function timezonePickerCtrl(currentUserInfo) {
		var ctrl = this;

		ctrl.$onInit = $onInit;
		ctrl.$onChanges = $onChanges;
		ctrl.onSelectionChanged = onSelectionChanged;
		ctrl.shortDisplayNameOfTheSelected = shortDisplayNameOfTheSelected;

		function $onInit() {
			populateTimezoneList();
			ctrl.selectedTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
		    onSelectionChanged();
		}

		function $onChanges(changesObj) {
			if (!changesObj.availableTimezones) return;
			populateTimezoneList();

			var ianaList = ctrl.timezoneList.map(function(z) {
				return z.ianaId;
			});

			if (ianaList.indexOf(ctrl.selectedTimezone) < 0) {
				ctrl.selectedTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
				onSelectionChanged();
			}
		}

		ctrl.isSelectedEnabled = function(timezoneId) {
			return timezoneId && (timezoneId.indexOf('invalidIanaId') < 0);
		}

		function populateTimezoneList() {
			var defaultTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var defaultTimezoneName = currentUserInfo.CurrentUserInfo().DefaultTimeZoneName;

			var timezoneDict = {};
			timezoneDict[defaultTimezone] = defaultTimezoneName;
			ctrl.availableTimezones.forEach(function (z) {
				if (z.IanaId === "") {
					timezoneDict['invalidIanaId' +' '+ z.DisplayName] = z.DisplayName;
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

		function shortDisplayNameOfTheSelected() {
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

		function onSelectionChanged() {
			ctrl.onPick({ timezone: ctrl.selectedTimezone });
		}
	}

})();