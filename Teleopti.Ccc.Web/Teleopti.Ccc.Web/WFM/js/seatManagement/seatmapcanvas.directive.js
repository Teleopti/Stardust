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

//Robtodo: move to seperate file and make generic
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

//Robtodo: move to seperate file
(function() {
	
	/*  override of fabric.util.getScrollLeftTop */
	fabric.util.getScrollLeftTop = function(element, upperCanvasEl) {
		
		var firstFixedAncestor,
			origElement,
			left = 0,
			top = 0,
			docElement = fabric.document.documentElement,
			body = fabric.document.body || {
				scrollLeft: 0, scrollTop: 0
			};

		origElement = element;
		
		while (element && element.parentNode && !firstFixedAncestor) {

			element = element.parentNode;

			if (element.nodeType === 1 &&
				fabric.util.getElementStyle(element, 'position') === 'fixed') {
				firstFixedAncestor = element;
			}

			if (element.nodeType === 1 &&
				origElement !== upperCanvasEl &&
				fabric.util.getElementStyle(element, 'position') === 'absolute') {
				left = 0;
				top = 0;
			}
			else if (element === fabric.document) {
				// overrride - was not calculating scroll position correctly!
				//left = body.scrollLeft || docElement.scrollLeft || 0;
				//top = body.scrollTop || docElement.scrollTop || 0;
				
				left = docElement.scrollLeft || 0;
				top = docElement.scrollTop || 0;
			}
			else {
				left += element.scrollLeft || 0;
				top += element.scrollTop || 0;
			}
		}

		return { left: left, top: top };
	}

}());
