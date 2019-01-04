'use strict';

(function() {
	angular
		.module('wfm.requests')
		.directive('requestsScheduleTable', scheduleTableDirective)
		.controller('requestsScheduleTableController', ['Toggle',requestsScheduleTableController]);

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
			controller: 'requestsScheduleTableController', 
			templateUrl: 'app/requests/html/requests-schedule-table.html'
		};
	}
	function requestsScheduleTableController(toggleService) {
		var vm = this;
		vm.enableStory79412 = toggleService.WFM_Request_Show_Shift_for_ShiftTrade_Requests_79412;
		vm.hideMeetingDetails = toggleService.WFM_Request_Show_Shift_for_Absence_Requests_79008;
	}
})();
