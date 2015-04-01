'use strict';

(function () {

	var directive = function () {

		return {
			controller: seatMapCanvasDirectiveController,
			controllerAs: 'vm',
			//bindToController: true,
			templateUrl: "js/seatManagement/html/seatmapcanvas.html",
			link: linkFunction
			
		};
	};

	angular.module('wfm.seatMap')
		.directive('seatmapCanvas', directive);
	


	function linkFunction(scope, element, attributes, vm) {

		//scope.vm.xxx
		
		//vm.xxx

	};

	function seatMapCanvasDirectiveController($document, $window, canvasUtils, canvasEditor) {

		var vm = this;
		vm.allowEdit = false;
		var canvas = new fabric.CanvasWithViewport('c');

		canvasUtils.setupCanvas(canvas);
		init();

		vm.toggleMoveMode = function(){
			canvasUtils.toggleMoveMode(canvas);
		};


		vm.toggleEditMode = function () {
			vm.allowEdit = !vm.allowEdit;
			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
		};

		function init() {
			
			$(document).ready(function () {
				$(window).resize(resize);
			});

			resize();
			createListenersKeyboard();
			canvasEditor.AddImage(canvas, 'seat.svg');
			
		};

		function resize() {
			canvasUtils.resize(canvas);
		};


		function createListenersKeyboard() {
			document.onkeydown = onKeyDownHandler;
		};

		 function onKeyDownHandler(event) {		
			//Robtodo: edit/non edit mode
			//if (self.allowEdit()) {
		 	canvasEditor.OnKeyDownHandler(canvas, event);
			//}

		};
		
	};

	seatMapCanvasDirectiveController.$inject = [ '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService' ];
	
}());

