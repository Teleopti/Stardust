(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.component('teamsDatetimePicker',
		{
			templateUrl: 'app/teamSchedule/html/teamsDatetimePicker.tpl.html',
			controller: TeamsDatetimePickerCtrl,
			bindings: {
				datetime: '<',
				onUpdate: '&'
			}
		});

	TeamsDatetimePickerCtrl.$inject = ['$locale'];

	function TeamsDatetimePickerCtrl($locale) {
		var ctrl = this;

		ctrl.$onInit = function () {
			ctrl.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
			ctrl.meridians = $locale.DATETIME_FORMATS.AMPMS;
		};

		ctrl.onDateChange = function () {
			ctrl.onUpdate({datetime: ctrl.datetime});
		};

		ctrl.onTimeChange = function () {
			ctrl.onUpdate({datetime: ctrl.datetime});
		};
	}
})();
