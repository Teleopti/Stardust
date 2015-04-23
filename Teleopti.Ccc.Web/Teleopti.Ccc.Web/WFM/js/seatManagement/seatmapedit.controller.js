'use strict';

(function () {

	angular.module('wfm.seatPlan')
		.controller('SeatMapEditCtrl', seatMapEditDirectiveController);

	seatMapEditDirectiveController.$inject = [
		'$scope', '$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'growl'
	];

	function seatMapEditDirectiveController($scope, $document, $window, utils, editor, growl) {

		var vm = this;
		vm.isInEditMode = false;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.showFileDialog = false;
		vm.fileCallbackFunction = null;

		init();

		function init() {
			createDocumentListeners();
		};

		vm.save = saveData;

		vm.addImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.addChosenImage;
		};

		vm.addChosenImage = function (image) {
			vm.showFileDialog = false;
			editor.addImage(canvas(), image);
		};

		vm.setBackgroundImage = function () {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.setChosenBackgroundImage;
		};

		vm.setChosenBackgroundImage = function (image) {
			vm.showFileDialog = false;
			editor.setBackgroundImage(canvas(), image);
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
			editor.addSeat(canvas(), false);
		};

		vm.addText = function () {
			editor.addText(canvas(), 'Double click here to edit text');
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

		vm.editButtonClick = function () {
			if (vm.isInEditMode) {
				vm.save();
				return;
			}
			vm.toggleEditMode();
		}

		vm.toggleEditMode = function () {
			vm.isInEditMode = !vm.isInEditMode;
			vm.parentVm.isInEditMode = vm.isInEditMode;

			utils.setSelectionMode(canvas(), vm.isInEditMode);
			if (!vm.isInEditMode) {
				vm.menuState = 'closed';
				refreshSeatMap();
			} 

			
			utils.resize(canvas(), vm.isInEditMode);
		};

		vm.delete = function () {
			editor.remove(canvas());
		};

		vm.flip = function (horizontal) {
			editor.flip(canvas(), horizontal);
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
			vm.parentVm.refreshSeatMap();
		};

		function canvas() {
			return vm.parentVm.getCanvas();
		};

		function createDocumentListeners() {
			document.onkeydown = onKeyDownHandler;
		};

		function onKeyDownHandler(event) {
			if (vm.isInEditMode) {
				editor.onKeyDownHandler(canvas(), event);
			}

		};

		function saveData() {
			vm.isLoading = true;

			var data = {
				SeatMapData: JSON.stringify(canvas()),
				Id: vm.parentVm.seatMapId,
				ChildLocations: utils.getLocations(canvas()),
				Seats: utils.getSeats(canvas()),
			}

			editor.save(data, onSaveSuccess);
		};

		function onSaveSuccess() {

			growl.success("<i class='mdi mdi-thumb-up'></i> Seat map saved successfully.", {
				ttl: 5000,
				disableCountDown: true
			});

			vm.toggleEditMode();
			refreshSeatMap();
		};
	
	};


}());

