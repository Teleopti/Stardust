



(function () {


	var directive = function () {

		return {
			controller: seatMapCanvasCtl,
			templateUrl: "html/seatManagement/seatmapcanvas.html",
			link: function (scope, elem, attrs) { }
		};
	};

	var seatMapCanvasControllerFunction = function ($scope, $document, $window, seatMapCanvasUtilsService) {

		var canvas = new fabric.CanvasWithViewport('c');

		$scope.toggleMoveMode = onMoveModeToggle;

		init();

		function onMoveModeToggle() {
			seatMapCanvasUtilsService.toggleMoveMode(canvas);
		};

		function init() {
			canvas.isGrabMode = false;

			$(document).ready(function () {
				$(window).resize(resize);
			});

			resize();
		};

		function resize() {
			seatMapCanvasUtilsService.resize(canvas);
		};
	};

	var seatMapCanvasCtl = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', seatMapCanvasControllerFunction
	];


	angular.module('wfm.seatMap')
		.directive('seatmapCanvas', [directive]);

}());



