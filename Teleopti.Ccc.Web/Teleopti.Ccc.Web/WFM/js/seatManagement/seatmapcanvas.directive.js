'use strict';

(function () {

	var directive = function () {

		return {
			controller: seatMapCanvasDirectiveController,
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
		window.resize(vm.resize);
		
	};

	seatMapCanvasDirectiveController.$inject = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'growl'
	];

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, canvasEditor, growl) {

		var vm = this;
		var canvas = new fabric.CanvasWithViewport('c');

		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.readonly = false;
		vm.allowEdit = false;
		vm.seatMapId = null;
		vm.parentId = null;
		vm.newLocationName = '';
		vm.fileCallbackFunction = null;

		init();

		vm.toggleMoveMode = function () {
			canvasUtils.toggleMoveMode(canvas);
		};


		vm.toggleEditMode = function () {
			if (vm.readonly) {
				return;
			}
			vm.allowEdit = !vm.allowEdit;
			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
			if (!vm.allowEdit) {
				
				refreshSeatMap();
			}
		};

		vm.addSeat = function () {
			canvasEditor.addSeat(canvas, false);
		};

		vm.addImage = function() {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.addChosenImage;
		};

		vm.addChosenImage = function(image) {
			vm.showFileDialog = false;
			canvasEditor.addImage(canvas, image);
		};

		vm.setBackgroundImage = function() {
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

		vm.addNamedLocation = function() {
			vm.showLocationDialog = false;
			canvasEditor.addLocation(canvas, vm.newLocationName);
		};

		vm.showLocationDialog = false;

		vm.save = saveData;

		vm.resize = resize;

		vm.handleBreadcrumbClick = function (id) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(id, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		vm.group = function() {
			canvasEditor.group(canvas);
		};

		vm.ungroup = function() {
			canvasEditor.ungroup(canvas);
		};

		vm.sendToBack = function() {
			canvasEditor.sendToBack(canvas);
		};

		vm.sendBackward = function() {
			canvasEditor.sendBackward(canvas);
		};

		vm.bringForward = function() {
			canvasEditor.bringForward(canvas);
		};

		vm.bringToFront = function () {
			canvasEditor.bringToFront(canvas);
		};

		vm.alignLeft = function() {
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

		function init() {

			fabric.Object.prototype.transparentCorners = false;
			fabric.Object.prototype.lockScalingFlip = true;
			fabric.Object.prototype.hasBorders = false;

			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
			canvasUtils.setupCanvas(canvas);
			setupHandleLocationClick();

			resize();

			createListenersKeyboard();
			vm.isLoading = true;
			canvasUtils.loadSeatMap(null, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function refreshSeatMap() {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(vm.seatMapId, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function resize() {
			canvasUtils.resize(canvas, vm.allowEdit);
		};

		//Robtodo: refactor - use $document and perhaps move to edit service.
		function createListenersKeyboard() {
			document.onkeydown = onKeyDownHandler;
		};

		function onKeyDownHandler(event) {
			if (vm.allowEdit) {
				canvasEditor.onKeyDownHandler(canvas, event);
			}

		};

		function setupHandleLocationClick() {
			canvas.on('mouse:down', function (e) {
				if (!vm.allowEdit) {
					var location = canvasUtils.getFirstObjectOfTypeFromCanvasObject(e.target, "location");
					if (location != null) {
						loadSeatMapOnLocationClick(location);
					}
				};
			});
		};

		function loadSeatMapOnLocationClick(location) {
			vm.isLoading = true;
			canvasUtils.loadSeatMap(location.id, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapNoSeatMapJson);
		};

		function onLoadSeatMapSuccess(data) {

			vm.parentId = data.ParentId;
			vm.seatMapId = data.Id;
			vm.breadcrumbs = data.BreadcrumbInfo;
			vm.seatPriority = data.seatPriority;

			//self.ResetZoom();
			//self.CacheObjectsAsImages();
			vm.isLoading = false;
			$scope.$apply(); // required to update bindings
		};

		function onLoadSeatMapNoSeatMapJson(data) {

			if (data) {
				vm.parentId = data.ParentId;
				vm.seatMapId = data.Id;
				vm.breadcrumbs = data.BreadcrumbInfo;
				vm.seatPriority = data.seatPriority;
			}

			vm.isLoading = false;
			//		//self.ResetZoom();
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


			vm.allowEdit = false;
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

