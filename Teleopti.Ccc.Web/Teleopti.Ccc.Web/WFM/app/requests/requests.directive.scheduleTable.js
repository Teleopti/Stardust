'use strict';

(function() {
	angular
		.module('wfm.requests')
		.directive('requestsScheduleTable', scheduleTableDirective);

	function scheduleTableDirective() {
		return {
			scope: {
				schedules: '=',
				shifts: '=',
				times:'='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/requests/html/requests-schedule-table.html'
		};
	}
})();
