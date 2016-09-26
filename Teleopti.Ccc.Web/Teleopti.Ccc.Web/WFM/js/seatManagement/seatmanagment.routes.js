(function () {
	'use strict';

	angular
	.module('wfm.seatPlan')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html'
		})
	}
})();
