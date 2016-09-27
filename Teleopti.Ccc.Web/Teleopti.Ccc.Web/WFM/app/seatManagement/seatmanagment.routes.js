(function () {
	'use strict';

	angular
	.module('wfm.seatPlan')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'app/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'app/seatManagement/html/seatmap.html'
		})
	}
})();
