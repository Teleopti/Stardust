'use strict';

(function () {

	angular.module('wfm.seatMap')
		.controller('SeatMapCanvasCtrl', seatMapCanvasDirectiveController);

	seatMapCanvasDirectiveController.$inject = ['$scope', '$document', '$window', 'seatMapCanvasUtilsService', '$timeout'];
	seatMapCanvasDirectiveController.$inject = ['$scope', '$document', '$window', 'seatMapCanvasUtilsService', '$timeout'];

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, $timeout) {

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
		vm.previousSelectedSeatIndex = 0;
		vm.selectTopLeftSeat = false;
		vm.zoomData = { min: 0.1, max: 2, step: 0.05, zoomValue: 1 };

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

			$scope.$on('sidenav:toggle', function () {
				resize();
			});

			resize();

			createDocumentListeners();
			vm.isLoading = true;
			vm.selectTopLeftSeat = true;

			canvasUtils.loadSeatMap(null, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
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

			if (vm.isInEditMode) return;

			vm.isLoading = true;
			vm.selectTopLeftSeat = true;

			canvasUtils.loadSeatMap(id, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		vm.refreshSeatMap = function (selectTopLeftSeat) {
			vm.selectTopLeftSeat = selectTopLeftSeat ? true : false;
			vm.isLoading = true;
			canvasUtils.loadSeatMap(vm.seatMapId, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
			resize();
		};

		vm.onChangeOfDate = function () {
			vm.refreshSeatMap(true);
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

		function setupHandleSeatAndLocationClick() {
			setupClickHandler('location', loadSeatMapOnLocationClick);
			setupClickHandler('seat', loadOccupancyForSeat);
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
		};

		function onSeatOccupancyLoaded(occupancyDetails, seat) {

			if (occupancyDetails && occupancyDetails.length) {
				vm.occupancydetails = occupancyDetails;
			} else {
				createEmptyOccupancyDetail(seat);
			}
		};

		function selectSeatToViewOccupancy(seat) {
			var oldSelection = vm.currentSeat;
			if (oldSelection !== undefined && oldSelection !== null) {
				oldSelection.stroke = null;
				oldSelection.strokeDashArray = [];
			}
			seat.stroke = '#ec38a3';
			seat.strokeDashArray = [5, 5];

			vm.currentSeat = seat;
		};

		function loadOccupancyForSeat(seat) {
			if (vm.showOccupancy) {
				selectSeatToViewOccupancy(seat);

				var seats = canvasUtils.getSeats(canvas);

				seats.forEach(function (currentSeat, seatIndex) {
					if (currentSeat.Id == seat.id)
						vm.previousSelectedSeatIndex = seatIndex;
				});

				canvasUtils.loadSeatDetails(seat.id, vm.selectedDate).then(function (result) {
					onSeatOccupancyLoaded(result, seat);
				});
			}
		};

		function createEmptyOccupancyDetail(seat) {
			vm.occupancydetails = [
			{
				SeatName: seat.name,
				IsEmpty: true
			}];
		};

		function loadSeatMapOnLocationClick(location) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(location.id, vm.selectedDate, canvas, false, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function selectSeat(seatIndex, isFirstSeat) {
			if (vm.showOccupancy) {
				var seat = canvasUtils.getSeatObject(canvas, seatIndex, isFirstSeat);

				if (seat != null) {
					loadOccupancyForSeat(seat);
				} else {
					vm.occupancydetails = undefined;
					vm.currentSeat = null;
				}
			}
		}

		function resetOnLoad(data) {

			if (data) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
			}

			if (vm.showOccupancy) {
				canvasUtils.showSeatBooking(canvas, data.Seats);
				canvasUtils.ungroupObjectsSoTheyCanBeIndividuallySelected(canvas);
			}

			selectSeat(vm.previousSelectedSeatIndex, vm.selectTopLeftSeat);
			vm.previousSelectedSeatIndex = 0;

			resetZoom();
			vm.isLoading = false;
		};

		function onLoadSeatMapSuccess(data) {
			resetOnLoad(data);

			$timeout(function () {
				$scope.$apply();
			});
		};

		function onLoadSeatMapNoSeatMapJson(data) {
			resetOnLoad(data);
		};
	};
}());

