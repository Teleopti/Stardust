'use strict';

(function() {
	angular.module('wfm.seatMap').directive('seatmapCanvas', seatmapCanvasDirective);

	function seatmapCanvasDirective() {
		return {
			controller: 'SeatMapCanvasController',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				toggle: '&toggle',
				selectedDate: '=date'
			},
			templateUrl: 'app/seatManagement/html/seatmapcanvas.html',
			link: function(scope, element, attributes, vm) {
				vm.showEditor = 'edit' in attributes;
				vm.isInEditMode = 'edit' in attributes;

				vm.showOccupancy = 'occupancy' in attributes;

				vm.init();
			}
		};
	}
})();
