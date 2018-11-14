﻿'use strict';

(function() {
	angular
		.module('wfm.requests')
		.directive('requestsScheduleTable', scheduleTableDirective)
		.controller('requestsScheduleTableController', [requestsScheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				schedules: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'requestsScheduleTableController',
			templateUrl: 'app/requests/html/requests-schedule-table.html'
		};
	}
	function requestsScheduleTableController() {}
})();
