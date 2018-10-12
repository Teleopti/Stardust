'use strict';

(function() {
	angular.module('wfm.seatMap').directive('seatmapOccupancyDetail', seatmapOccupancyDetailDirective);
	seatmapOccupancyDetailDirective.$inject = ['$translate'];

	function seatmapOccupancyDetailDirective($translate) {
		return {
			controller: 'SeatMapOccupancyController',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapOccupancyDetail', '^seatmapCanvas', '^wfmRightPanel'],
			scope: {
				scheduleDate: '=',
				refreshSeatMap: '&'
			},
			restrict: 'E',
			templateUrl: 'app/seatManagement/html/seatmapoccupancylist.html',
			link: function(scope, element, attributes, controllers) {
				var vm = controllers[0];
				vm.parentVm = controllers[1];
				vm.rightPanelVm = controllers[2];

				vm.parentVm.rightPanelOptions.panelTitle = ' '; //remove when styleguide update
				vm.parentVm.rightPanelOptions.sidePanelTitle = $translate.instant('SeatBookingsList');
				vm.init();
			}
		};
	}
})();
