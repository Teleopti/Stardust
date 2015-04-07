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
		//vm.xxx
	};

	function seatMapCanvasDirectiveController($scope, $document, $window, canvasUtils, canvasEditor) {

		var vm = this;
		var canvas = new fabric.CanvasWithViewport('c');

		vm.isLoading = true;
		vm.breadcrumbs = [];
		vm.allowEdit = false;
		vm.seatMapId = null;
		vm.parentId = null;

		init();

		vm.toggleMoveMode = function () {
			canvasUtils.toggleMoveMode(canvas);
		};

		vm.toggleEditMode = function () {
			vm.allowEdit = !vm.allowEdit;
			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
			if (!vm.allowEdit) {
				refreshSeatMap();
			}
		};

		vm.addSeat = function () {
			canvasEditor.addSeat(canvas, false);
		};

		vm.save = saveData;


		vm.handleBreadcrumbClick = function (id) {
			canvasUtils.loadSeatMap(id, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapFailure);
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
			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
			canvasUtils.setupCanvas(canvas);
			setupHandleLocationClick();

			$(document).ready(function () {
				$(window).resize(resize);
			});

			resize();
			createListenersKeyboard();
			canvasUtils.loadSeatMap(null, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapFailure);
		};

		function refreshSeatMap() {
			canvasUtils.loadSeatMap(vm.seatMapId, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapFailure);
		};

		function resize() {
			canvasUtils.resize(canvas);
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

			canvasUtils.loadSeatMap(location.id, canvas, vm.allowEdit, onLoadSeatMapSuccess, onLoadSeatMapFailure);
		};

		function onLoadSeatMapSuccess(data) {

			vm.parentId = data.ParentId;
			vm.seatMapId = data.Id;
			vm.breadcrumbs = data.BreadcrumbInfo;

			//self.ResetZoom();
			//self.CacheObjectsAsImages();
			vm.isLoading = false;
			$scope.$apply(); // required to update bindings
		};

		function onLoadSeatMapFailure() {
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
			alert('saved');

			//Robtodo: Success Message
			vm.allowEdit = false;
			refreshSeatMap();
		};

		function getLocations() {
			var childLocations = [];
			var locations = canvasUtils.getObjectsByType(canvas, 'location');
			for (var i in locations) {
				childLocations.push(
				{
					Id: locations[i].id,
					Name: locations[i].name,
					IsNew: locations[i].isNew
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

	seatMapCanvasDirectiveController.$inject = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService'
	];

}());

