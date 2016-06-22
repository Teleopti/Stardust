'use strict';

(function () {
	angular.module('wfm.requests')
		.directive('requestsScheduleTable', scheduleTableDirective)
		.controller('requestsScheduleTableCtrl', [requestsScheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				schedules:'='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'requestsScheduleTableCtrl',
			templateUrl: "requests-schedule-table.html"
		};
	};
	function requestsScheduleTableController() {
	};
}());