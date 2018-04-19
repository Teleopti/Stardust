(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.component('teamsDatetimePicker',
		{
			templateUrl: 'app/teamSchedule/html/teamsDatetimePicker.tpl.html',
			controller: TeamsDatetimePickerCtrl,
			require: {
				ngModelCtrl: 'ngModel'
			},
			bindings: {
				ngModel: '=',
				timezone: '<?',
				minuteStep: '<?'
			}
		});

	TeamsDatetimePickerCtrl.$inject = ['$scope', 'serviceDateFormatHelper'];

	function TeamsDatetimePickerCtrl($scope, serviceDateFormatHelper) {
		var ctrl = this;
		ctrl.date = moment(serviceDateFormatHelper.getDateOnly(ctrl.ngModel)).toDate();
		ctrl.ngModelChange = function () {
			ctrl.ngModelCtrl.$setViewValue(ctrl.ngModel);
		};
	}
})();
