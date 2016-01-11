'use strict';

(function () {

	angular.module('wfm.seatPlan')
		.controller('SeatMapEditCtrl', seatMapEditDirectiveController);

	seatMapEditDirectiveController.$inject = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'growl'
	];

	function seatMapEditDirectiveController($scope, $document, $window, utils, editor, growl) {

		var vm = this;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.showFileDialog = false;
		vm.fileCallbackFunction = null;
		vm.hasCopiedObjectBool = false;
		
		vm.init = function () {
			canvas().on('object:selected', function (e) {
				vm.parentVm.getActiveObjects();
			});

			canvas().on('selection:created', function (e) {
				if (e.e && e.e.shiftKey) {
					vm.parentVm.getActiveObjects();
				}
			});

			canvas().on('before:selection:cleared', function (e) {
				vm.parentVm.rightPanelOption.panelState = false;
			});

			createDocumentListeners();
		};

		vm.save = saveData;

		vm.addImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.addChosenImage;
		};

		vm.addChosenImage = function (image) {
			var imagePreviewElement = document.getElementById('image-preview');
			var sizeFromImagePreview = { height: imagePreviewElement.height, width: imagePreviewElement.width };
			vm.showFileDialog = false;
			editor.addImage(canvas(), image, sizeFromImagePreview);
		};

		vm.setBackgroundImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.setChosenBackgroundImage;
		};

		vm.clearBackgroundImage = function () {
			vm.setChosenBackgroundImage(null);
		};

		vm.setChosenBackgroundImage = function (image) {
			var imagePreviewElement = document.getElementById('image-preview');
			vm.showFileDialog = false;
			editor.setBackgroundImage(canvas(), image, imagePreviewElement);
		};

		vm.addLocation = function () {
			vm.newLocationName = '';
			vm.showLocationDialog = true;
		};

		vm.addNamedLocation = function () {
			vm.showLocationDialog = false;
			editor.addLocation(canvas(), vm.newLocationName);
		};

		vm.addSeat = function () {
			editor.addSeat(canvas(), false, afterAddSeat);
		};

		vm.addText = function() {
			editor.addText(canvas(), 'Double click here to edit text');
		};

		vm.copy = function () {
			vm.hasCopiedObjectBool = true;
			editor.copy(canvas());
		};

		vm.paste = function () {
			editor.paste(canvas(), afterPaste);
		};

		vm.cut = function () {
			editor.copy(canvas());
			editor.remove(canvas());
		}

		vm.group = function () {
			editor.group(canvas());
		};

		vm.ungroup = function () {
			editor.ungroup(canvas());
		};

		vm.sendToBack = function () {
			editor.sendToBack(canvas());
		};

		vm.sendBackward = function () {
			editor.sendBackward(canvas());
		};

		vm.bringForward = function () {
			editor.bringForward(canvas());
		};

		vm.bringToFront = function () {
			editor.bringToFront(canvas());
		};

		vm.alignLeft = function () {
			editor.alignLeft(canvas());
		};

		vm.alignRight = function () {
			editor.alignRight(canvas());
		};

		vm.alignTop = function () {
			editor.alignTop(canvas());
		};

		vm.alignBottom = function () {
			editor.alignBottom(canvas());
		};

		vm.rotate45 = function () {
			editor.rotate45(canvas());
		};

		vm.spaceActiveGroupVertical = function () {
			editor.spaceActiveGroupVertical(canvas());
		};

		vm.spaceActiveGroupHorizontal = function () {
			editor.spaceActiveGroupHorizontal(canvas());
		};

		vm.delete = function () {
			var removeObjectIds = editor.remove(canvas());
			vm.parentVm.seats = vm.parentVm.seats.filter(function (seat) {
				return removeObjectIds.indexOf(seat.Id) == -1;
			});
		};

		vm.flip = function (horizontal) {
			editor.flip(canvas(), horizontal);
		}

		vm.hasObjectSelected = function() {
			return (canvas().getActiveObject() || canvas().getActiveGroup()) != null;
		};

		vm.hasChanges = function () {
			var seatPropertiesChanged = vm.parentVm.loadedSeatsData != JSON.stringify(vm.parentVm.seats);
			return (vm.parentVm.loadedJsonData != JSON.stringify(canvas()) || seatPropertiesChanged)
				&& (vm.parentVm.loadedJsonData == undefined ? canvas()._objects.length != 0 : true);
		};

		vm.refreshSeatMap = function () {
			vm.parentVm.refreshSeatMap();
		};

		function canvas() {
			return vm.parentVm.getCanvas();
		};

		function afterAddSeat(newSeat) {
			vm.parentVm.seats.push(newSeat);
			createDocumentListeners();
		};

		function afterPaste(seats) {
			vm.parentVm.seats = vm.parentVm.seats.concat(seats);
			createDocumentListeners();
		};

		function createDocumentListeners() {
			document.onkeydown = onKeyDownHandler;
		};

		function suspendDocumentListeners() {
			document.onkeydown = null;
		};

		function onKeyDownHandler(event) {
			performKeyDownAction(canvas(), event);
		};

		function preventDefaultEvent(event) {
			// ie <11 doesnt have e.preventDefault();
			if (event.preventDefault) event.preventDefault();
			event.returnValue = false;
		};

		function performKeyDownAction(canvas, event) {
			if (vm.parentVm.isLoading) return;
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 45: // insert
					suspendDocumentListeners();
					vm.addSeat();
					break;
				case 46: // delete
					vm.delete();
					break;
				case 67: // Ctrl+C
					if (event.ctrlKey) {
						preventDefaultEvent(event);
						vm.copy();
					}
					break;

				case 73: // I
					if (event.altKey) {
						$scope.$apply(vm.addImage());
					}
					break;

				case 76: // L
					if (event.altKey) {
						$scope.$apply(vm.addLocation());
					}
					break;

				case 86: // Ctrl+V
					if (event.ctrlKey) {
						suspendDocumentListeners();
						preventDefaultEvent(event);
						vm.paste();
					}
					break;

				case 83:
					//Cntrl+S
					if (event.ctrlKey) {
						preventDefaultEvent(event);
						saveData();
					}
					if (event.altKey) {
						preventDefaultEvent(event);
						suspendDocumentListeners();
						vm.addSeat();
					}
					break;

				case 84: //T
					if (event.altKey) {
						preventDefaultEvent(event);
						vm.addText();
					}
					break;

				case 88: //X
					if (event.ctrlKey) {
						preventDefaultEvent(event);
						vm.cut();
					}

					break;
				default:
					break;
			}
		};

		function saveData() {
			if (!vm.hasChanges()) return;
			vm.parentVm.isLoading = true;

			var data = {
				SeatMapData: JSON.stringify(canvas()),
				Id: vm.parentVm.seatMapId,
				ChildLocations: utils.getLocations(canvas()),
				Seats: vm.parentVm.seats
			};

			editor.save(data, onSaveSuccess);
			vm.parentVm.rightPanelOption.panelState = false;
		};

		function onSaveSuccess() {

			growl.success("<i class='mdi mdi-thumb-up'></i> Seat map saved successfully.", {
				ttl: 5000,
				disableCountDown: true
			});

			vm.refreshSeatMap();
		};
	};
}());

