'use strict';

(function () {

	angular.module('wfm.seatPlan')
		.controller('SeatMapCanvasCtrl', seatMapCanvasDirectiveController);

	seatMapCanvasDirectiveController.$inject = ['$scope', '$document', '$window', 'seatMapCanvasUtilsService'];

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils) {

		var vm = this;
		vm.isInEditMode = false;
		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.showEditor = false;
		vm.showOccupancy = false;
		vm.seatMapId = null;
		vm.parentId = null;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.fileCallbackFunction = null;
		vm.scrollListen = false;
		vm.zoomData = { min: 0.1, max: 1, step: 0.05, zoomValue: 1 };
		
		var canvas = new fabric.CanvasWithViewport('c');
		init();

		function init() {
			fabric.util.addListener(document.getElementsByClassName('upper-canvas')[0], 'contextmenu', function (e) {				
				if (e.preventDefault) e.preventDefault();
				e.returnValue = false;
				return false;
			});

			fabric.Object.prototype.transparentCorners = false;
			fabric.Object.prototype.lockScalingFlip = true;
			fabric.Object.prototype.hasBorders = false;
			canvasUtils.setupCanvas(canvas);
			setupHandleSeatAndLocationClick();

			angular.element($window).bind('resize', function () {
				resize();
			});

			resize();

			createDocumentListeners();
			vm.isLoading = true;
			canvasUtils.loadSeatMap(null, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};
		
		vm.getDisplaySelectedDate = function () {
			return moment(vm.selectedDate).format('L');
		};

		vm.getCanvas = function() {
			return canvas;
		};

		vm.isMoveMode = function () {
			return canvas.isGrabMode;
		};
		vm.zoom = function () {
			canvasUtils.zoom(canvas, vm.zoomData.zoomValue);
		};

		vm.toggleMoveMode = function () {
			canvasUtils.toggleMoveMode(canvas);
		};
		
		vm.resize = resize;

		vm.handleBreadcrumbClick = function (id) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(id, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};
		
		vm.refreshSeatMap = function() {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(vm.seatMapId, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
			resize();
		};

		vm.onChangeOfDate = function() {
			vm.refreshSeatMap();
		};

		function resize() {
			canvasUtils.resize(canvas, false);
		};

		function scrollZooming() {
			vm.zoomData.zoomValue = canvasUtils.scrollZooming($window, canvas, vm.scrollListen, vm.zoomData);
			$scope.$apply();
		};

		function resetZoom() {
			vm.zoomData.zoomValue = 1;
			canvasUtils.zoom(canvas, vm.zoomData.zoomValue);
			canvasUtils.resetPosition(canvas);
		};

		function createDocumentListeners() {
			document.onmousewheel = scrollZooming;
		};

		function setupHandleSeatAndLocationClick() {
			setupClickHandler('location', loadSeatMapOnLocationClick);
			setupClickHandler('seat', loadOccupancyOnSeatClick);
		};

		function setupClickHandler(targetName, callback) {
			canvas.on('mouse:down', function (e) {
				if (!vm.isInEditMode) {
					var target = canvasUtils.getFirstObjectOfTypeFromCanvasObject(e.target, targetName);
					if (target != null) {
						callback(target);
					}
				};
			});
		}

		function loadOccupancyOnSeatClick(seat) {
			vm.currentSeat = seat;
			canvasUtils.loadSeatDetails(seat.id, vm.selectedDate).then(function (result) {
				if (result && result.length) vm.occupancydetails = result;
				else vm.occupancydetails = undefined;
			});
		};

		function loadSeatMapOnLocationClick(location) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(location.id, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function onLoadSeatMapSuccess(data) {

			vm.parentId = data.ParentId;
			vm.seatMapId = data.Id;
			vm.breadcrumbs = data.BreadcrumbInfo;
			resetZoom();
			vm.isLoading = false;
			if (vm.showOccupancy) {
				canvasUtils.applyOccupancyColoring(canvas, data.Seats);
			}

			$scope.$apply();
		};

		function onLoadSeatMapNoSeatMapJson(data) {

			if (data) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
			}

			vm.isLoading = false;
			resetZoom();
		};

	};
}());

