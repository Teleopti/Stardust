'use strict';

(function () {

	angular.module('wfm.seatMap')
		.controller('SeatMapCanvasCtrl', seatMapCanvasDirectiveController);

	seatMapCanvasDirectiveController.$inject = ['$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'PermissionsServiceRefact', 'NoticeService', '$timeout'];

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, editor, PermissionsServiceRefact, NoticeService, $timeout) {

		var vm = this;
		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.showSearchPeople = false;
		vm.seatMapId = null;
		vm.parentId = null;
		vm.roles = [];
		vm.seats = [];
		vm.locationPrefix = null;
		vm.locationSuffix = null;
		vm.prefixOrSuffixChanged = false;
		vm.activeSeats = [];
		vm.otherActiveObjects = [];
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.fileCallbackFunction = null;
		vm.scrollListen = false;
		vm.loadedSeatsData = [];
		vm.zoomData = { min: 0.1, max: 2, step: 0.05, zoomValue: 1 };
		vm.rightPanelOptions = {
			panelState: false,
			sidePanelTitle: "SeatProperties",
			showCloseButton: true,
			showBackdrop: false,
			showResizer: true,
			showPopupButton: true
		};
		vm.prefixSuffixPanelOptions = {
			panelState: false,
			sidePanelTitle: "LocationProperties",
			showCloseButton: true,
			showBackdrop: false,
			showResizer: true,
			showPopupButton: false
		};

		var canvas = new fabric.CanvasWithViewport('c');

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

			resize();

			angular.element($window).bind('resize', function () {
				resize();
			});

			$scope.$on('sidenav:toggle', function () {
				resize();
			});

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

		vm.getRoles = function () {
			PermissionsServiceRefact.roles.query().$promise.then(function (rolesData) {
				vm.roles = rolesData;
			});
		};

		vm.getActiveObjects = function () {
			vm.activeSeats = [];
			vm.otherActiveObjects = [];

			vm.rightPanelOptions.showPopupButton = true;

			canvasUtils.getActiveSeatObjects(canvas, vm.seats, vm.activeSeats);

			vm.otherActiveObjects = vm.otherActiveObjects.concat(canvasUtils.getActiveFabricObjectsByType(canvas, 'location'),
														canvasUtils.getActiveFabricObjectsByType(canvas, 'image'),
														canvasUtils.getActiveFabricObjectsByType(canvas, 'i-text'));

			vm.prefixSuffixPanelOptions.showPopupButton = false;
			vm.prefixSuffixPanelOptions.panelState = false;

			//TODO:currently we only support showing properties for seats
			if (vm.activeSeats.length > 0 && vm.otherActiveObjects.length == 0) {
				vm.rightPanelOptions.panelState = true;
			}

			if (vm.otherActiveObjects.length > 0) {
				vm.rightPanelOptions.showPopupButton = false;
				vm.rightPanelOptions.panelState = false;
			}
		};

		vm.handleBreadcrumbClick = function (id) {
			vm.isLoading = true;
			vm.rightPanelOptions.panelState = false;
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

		vm.updateSeatNumber = function(seat, newNumber) {
			canvasUtils.updateSeatNumber(canvas, seat, newNumber, vm.seats);
		}

		function onSaveSuccess() {
			NoticeService.success('Seat map saved successfully.', 5000, false);
			vm.refreshSeatMap();
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
			if (data.Id != undefined) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
				vm.loadedJsonData = data.SeatMapJsonData;
				vm.loadedSeatsData = JSON.stringify(data.Seats);
				vm.seats = data.Seats;
				vm.locationPrefix = data.LocationPrefix;
				vm.locationSuffix = data.LocationSuffix;
			}

			//resetZoom();
			vm.isLoading = false;
			canvas.fire('seatmaplocation:loaded', { data: data });

			$timeout(function () {
				$scope.$apply();
				resize();
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
