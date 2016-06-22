'use strict';

(function () {
	angular.module('wfm.requests')
		.directive('requestsScheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['PersonSelection', '$scope', 'ScheduleManagement', scheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				schedules:'='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: "js/teamSchedule/html/scheduletable.html"
		};
	};
	function scheduleTableController() {
	};
}());