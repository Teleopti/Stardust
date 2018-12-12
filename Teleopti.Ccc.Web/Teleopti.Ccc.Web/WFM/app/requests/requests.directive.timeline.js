'use strict';

(function () {
	angular
		.module('wfm.requests')
		.directive('requestsScheduleTimeline', requestsScheduleTimelineDirective)
		.controller('requestsScheduleTimeLineController', [requestsScheduleTimeLineController]);

	function requestsScheduleTimelineDirective() {
		return {
			scope: {
				times: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'requestsScheduleTimeLineController',
			templateUrl: 'app/requests/html/timeline.html'
		};
	}
	function requestsScheduleTimeLineController() { }
})();

