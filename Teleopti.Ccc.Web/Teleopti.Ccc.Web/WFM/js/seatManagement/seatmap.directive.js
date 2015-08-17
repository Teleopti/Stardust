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

	function linkFunction(scope, element, attributes, vm) {
		vm.showEditor = 'edit' in attributes;
	};

	angular.module('wfm.seatMap')
		.directive('seatmapCanvas', directive);
		
}());






