'use strict';

(function () {

	angular.module('wfm.seatMap')
		.controller('SeatMapCanvasCtrl', seatMapCanvasDirectiveController);
		
seatMapCanvasDirectiveController.$inject = ['$scope', '$document', '$window', 'seatMapCanvasUtilsService', '$timeout', '$mdDialog'];




	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, $timeout, $mdDialog) {

		var vm = this;
		vm.isInEditMode = false;
		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.showEditor = false;
		vm.showOccupancy = false;
		vm.showSearchPeople = false;
		vm.seatMapId = null;
		vm.parentId = null;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.fileCallbackFunction = null;
		vm.scrollListen = false;
		vm.zoomData = { min: 0.1, max: 2, step: 0.05, zoomValue: 1 };

		var canvas = new fabric.CanvasWithViewport('c');
		//init();

		vm.init = function () {
			fabric.util.addListener(document.getElementsByClassName('upper-canvas')[0], 'contextmenu', function (e) {
				if (e.preventDefault) e.preventDefault();
				e.returnValue = false;
				return false;
			});

			fabric.Object.prototype.transparentCorners = false;
			fabric.Object.prototype.lockScalingFlip = true;
			canvas.hoverCursor = 'pointer';
			canvasUtils.setupCanvas(canvas);

			angular.element($window).bind('resize', function () {
				resize();
			});

			$scope.$on('sidenav:toggle', function () {
				resize();
			});

			resize();

			createDocumentListeners();
			vm.isLoading = true;
			canvasUtils.loadSeatMap(null, vm.selectedDate, canvas, vm.isInEditMode, vm.showOccupancy, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);

		};

		vm.getDisplaySelectedDate = function () {
			return moment(vm.selectedDate).format('L');
		};

		vm.getCanvas = function () {
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
			canvasUtils.loadSeatMap(id, vm.selectedDate, canvas, vm.isInEditMode, vm.showOccupancy, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		vm.refreshSeatMap = function () {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(vm.seatMapId, vm.selectedDate, canvas, vm.isInEditMode, vm.showOccupancy, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
			resize();
		};

		vm.onChangeOfDate = function () {
			vm.refreshSeatMap();
			vm.isDatePickerOpened = false;
		};

		function resize() {
			canvasUtils.resize(canvas);
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

		function setupLocationDoubleClickHandler() {
			setupObjectTypeDoubleClickHandler('location', loadSeatMapOnLocationClick);
		};

		function setupObjectTypeDoubleClickHandler(objectType, callback) {
			canvasUtils.getObjectsByType(canvas, objectType).forEach(function (object) {
				object.on('object:dblclick', function (e) {
					if (object != null) {
						callback(object);
					}
				});
			});
		};

		function loadSeatMapOnLocationClick(location) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(location.id, vm.selectedDate, canvas, vm.isInEditMode, vm.showOccupancy, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function resetOnLoad(data) {

			setupLocationDoubleClickHandler();

			if (data) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
				vm.loadedData = data.SeatMapJsonData;
			}

			resetZoom();
			vm.isLoading = false;
			canvas.fire('seatmaplocation:loaded', { data: data });

			$timeout(function () {
				$scope.$apply();
			});
		};

		function onLoadSeatMapSuccess(data) {
			resetOnLoad(data);

		};

		function onLoadSeatMapNoSeatMapJson(data) {
			resetOnLoad(data);
		};
	};
}());


