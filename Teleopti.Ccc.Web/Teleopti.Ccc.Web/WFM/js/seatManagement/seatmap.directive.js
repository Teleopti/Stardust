'use strict';

(function () {

	var directive = function () {

		return {
			controller: 'SeatMapCanvasCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/seatManagement/html/seatmapcanvas.html",
			link: linkFunction
		};
	};

	angular.module('wfm.seatMap')
		.directive('seatmapCanvas', directive);

	function linkFunction(scope, element, attributes, vm) {
		vm.readonly = 'readonly' in attributes;
	};
		
}());






