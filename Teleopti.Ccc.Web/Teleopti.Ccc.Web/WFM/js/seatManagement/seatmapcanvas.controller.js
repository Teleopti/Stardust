'use strict';

(function () {

	angular.module('wfm.seatPlan')
		.controller('SeatPlanCanvasCtrl', seatMapCanvasDirectiveController);

	seatMapCanvasDirectiveController.$inject = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'growl'
	];

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, canvasEditor, growl) {

		var vm = this;
		var canvas = new fabric.CanvasWithViewport('c');

		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.readonly = false;
		vm.isInEditMode = false;
		vm.seatMapId = null;
		vm.parentId = null;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.fileCallbackFunction = null;
		vm.scrollListen = false;
		vm.zoomData = { min: 0.1, max: 5, step: 0.05, zoomValue: 1 };

		init();

		function init() {

			fabric.Object.prototype.transparentCorners = false;
			fabric.Object.prototype.lockScalingFlip = true;
			fabric.Object.prototype.hasBorders = false;

			canvasUtils.setSelectionMode(canvas, vm.isInEditMode);
			canvasUtils.setupCanvas(canvas);
			setupHandleLocationClick();

			angular.element($window).bind('resize', function () {
				//vm.resize();
				resize();
			});

			$window.addEventListener('mousewheel', scrollZooming, false);

			resize();

			createListenersKeyboard();
			vm.isLoading = true;
			canvasUtils.loadSeatMap(null, canvas, vm.isInEditMode, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
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

		vm.toggleEditMode = function () {
			if (vm.readonly) {
				return;
			}
			vm.isInEditMode = !vm.isInEditMode;

			canvasUtils.setSelectionMode(canvas, vm.isInEditMode);
			if (!vm.isInEditMode) {
				vm.menuState = 'closed';
				refreshSeatMap();
			}

			resize();
		};
		
		vm.save = saveData;
		vm.resize = resize;

		vm.handleBreadcrumbClick = function (id) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(id, canvas, vm.isInEditMode, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		vm.addImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.addChosenImage;
		};

		vm.addChosenImage = function (image) {
			vm.showFileDialog = false;
			canvasEditor.addImage(canvas, image);
		};

		vm.setBackgroundImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.setChosenBackgroundImage;
		};

		vm.setChosenBackgroundImage = function (image) {
			vm.showFileDialog = false;
			canvasEditor.setBackgroundImage(canvas, image);
		};

		vm.addLocation = function () {
			vm.newLocationName = '';
			vm.showLocationDialog = true;
		};

		vm.addNamedLocation = function () {
			vm.showLocationDialog = false;
			canvasEditor.addLocation(canvas, vm.newLocationName);
		};

		vm.addSeat = function () {
			canvasEditor.addSeat(canvas, false);
		};

		vm.addText = function () {
			 canvasEditor.addText(canvas, 'Double click here to edit text');
		}

		vm.group = function () {
			 canvasEditor.group(canvas);
		};

		vm.ungroup = function () {
			 canvasEditor.ungroup(canvas);
		};

		vm.sendToBack = function () {
			 canvasEditor.sendToBack(canvas);
		};

		vm.sendBackward = function () {
			 canvasEditor.sendBackward(canvas);
		};

		vm.bringForward = function () {
			 canvasEditor.bringForward(canvas);
		};

		vm.bringToFront = function () {
			 canvasEditor.bringToFront(canvas);
		};

		vm.alignLeft = function () {
			 canvasEditor.alignLeft(canvas);
		};

		vm.alignRight = function () {
			 canvasEditor.alignRight(canvas);
		};

		vm.alignTop = function () {
			 canvasEditor.alignTop(canvas);
		};

		vm.alignBottom = function () {
			 canvasEditor.alignBottom(canvas);
		};

		vm.rotate45 = function() {
			canvasEditor.rotate45(canvas);
		};

		vm.spaceActiveGroupVertical = function() {
			canvasEditor.spaceActiveGroupVertical(canvas);
		};

		vm.spaceActiveGroupHorizontal = function() {
			canvasEditor.spaceActiveGroupHorizontal(canvas);
		};

		vm.editButtonClick = function () {
			if (vm.isInEditMode) {
				vm.save();
				return;
			}
			vm.toggleEditMode();
		}

		vm.menuState = 'closed';

		vm.floatingButtonClick = function (action) {
			action();
		};
		vm.chosen = {
			effect: 'slidein-spring',
			position: 'tr',
			method: 'click'
		};

		vm.buttons = [{
			label: 'Add Seat',
			icon: 'mdi-plus',
			action: vm.addSeat

		}, {
			label: 'Add Location',
			icon: ' mdi-tab-unselected',
			action: vm.addLocation
		}, {
			label: 'Add Image',
			icon: 'mdi-file-image',
			action: vm.addImage
		}, {
			label: 'Add Text',
			icon: ' mdi-tooltip-text',
			action: vm.addText
		}, {
			label: 'Set background',
			icon: 'mdi-file-image-box',
			action: vm.setBackgroundImage
		}];
		
		function refreshSeatMap() {
			vm.isLoading = true;
			vm.isInEditMode = false;
			canvasUtils.loadSeatMap(vm.seatMapId, canvas, vm.isInEditMode, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
			resize();
		};

		function resize() {
			canvasUtils.resize(canvas, vm.isInEditMode);
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

		function createListenersKeyboard() {
			document.onkeydown = onKeyDownHandler;
		};

		function onKeyDownHandler(event) {
			if (vm.isInEditMode) {
				canvasEditor.onKeyDownHandler(canvas, event);
			}

		};

		function setupHandleLocationClick() {
			canvas.on('mouse:down', function (e) {
				if (!vm.isInEditMode) {
					var location = canvasUtils.getFirstObjectOfTypeFromCanvasObject(e.target, "location");
					if (location != null) {
						loadSeatMapOnLocationClick(location);
					}
				};
			});
		};

		function loadSeatMapOnLocationClick(location) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(location.id, canvas, vm.isInEditMode, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function onLoadSeatMapSuccess(data) {
			vm.parentId = data.ParentId;
			vm.seatMapId = data.Id;
			vm.breadcrumbs = data.BreadcrumbInfo;
			vm.seatPriority = data.seatPriority;
			resetZoom();
			vm.isLoading = false;

			$scope.$apply();
		};

		function onLoadSeatMapNoSeatMapJson(data) {

			if (data) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
				vm.seatPriority = data.seatPriority;
			}

			vm.isLoading = false;
			resetZoom();
		};



		function saveData() {

			vm.isLoading = true;

			var data = {
				SeatMapData: JSON.stringify(canvas),
				Id: vm.seatMapId,
				ChildLocations: getLocations(),
				Seats: getSeats()
			}

			canvasEditor.save(data, onSaveSuccess);
		};

		function onSaveSuccess() {

			growl.success("<i class='mdi mdi-thumb-up'></i> Seat map saved successfully.", {
				ttl: 5000,
				disableCountDown: true
			});
			
			refreshSeatMap();
		};

		function getLocations() {
			var childLocations = [];
			var locations = canvasUtils.getObjectsByType(canvas, 'location');
			for (var i in locations) {
				var location = locations[i];
				childLocations.push(
				{
					Id: location.id,
					Name: location.name,
					IsNew: (location.isNew === undefined) ? false : true
				});
			}
			return childLocations;
		};

		function getSeats() {
			var seats = [];
			var seatObjects = canvasUtils.getObjectsByType(canvas, 'seat');
			for (var i in seatObjects) {
				var seat = seatObjects[i];
				seats.push(
				{
					Id: seat.id,
					Name: seat.name,
					Priority: seat.priority,
					IsNew: (seat.isNew === undefined) ? false : true
				});
			}
			return seats;
		};
	};


}());

