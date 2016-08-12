'use strict';

(function () {
	angular.module('wfm.requests')
		.controller('workingHoursPickerExtendController', workingHoursPickerExtendController)
		.directive('workingHoursPickerWithCloseDay', withCloseDayDirective);

	function withCloseDayDirective(workingHoursPickerDirective) {
		return angular.extend({}, workingHoursPickerDirective[0], {
			scope: {
				workingHours: '='
			},
			controller: 'workingHoursPickerExtendController',
			templateUrl: 'js/requests/html/working-hours-picker-extend.tpl.html'
		});
	};
	function workingHoursPickerExtendController($scope) {
		$scope.toggleCloseDay = toggleCloseDay;

		function toggleCloseDay(index) {
			if (!$scope.workingHours[index]) return;
			angular.forEach($scope.workingHours[index].WeekDaySelections, function (weekDaySelection) {
				if (weekDaySelection.IsClosed == undefined) {
					weekDaySelection.IsClosed = true;
				} else {
					weekDaySelection.IsClosed = !weekDaySelection.IsClosed;
				}
			});
		}
	}
}());