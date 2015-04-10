'use strict';

(function () {

	var directive = function () {

		return {
			controller: 'SeatPlanCanvasCtrl',
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


(function () {

	var directive = function () {

		return {
			//Robtodo: remove dependancy on parent controller to genericize this.
			//	controller: fileDialogController,
			//controllerAs: 'vm',
			//bindToController: true,
			templateUrl: "js/seatManagement/html/filedialog.html",
			link: linkFunction
		};
	};

	angular.module('wfm.seatMap')
		.directive('fileDialog', directive);

	function linkFunction(scope, element, attributes, vm) {
		
	};

}());

