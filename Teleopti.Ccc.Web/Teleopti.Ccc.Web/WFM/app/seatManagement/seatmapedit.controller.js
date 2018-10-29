'use strict';

(function() {
	angular.module('wfm.seatPlan').controller('SeatMapEditController', seatMapEditDirectiveController);

	seatMapEditDirectiveController.$inject = [
		'$scope',
		'$translate',
		'seatMapCanvasUtilsService',
		'seatMapCanvasEditService',
		'NoticeService'
	];

	function seatMapEditDirectiveController(
		$scope,
		$translate,
		seatMapCanvasUtils,
		seatMapCanvasEditSvc,
		NoticeService
	) {
		var vm = this;
		vm.newLocationName = '';
		vm.showLocationDialog = false;
		vm.showFileDialog = false;
		vm.fileCallbackFunction = null;
		vm.hasCopiedObjectBool = false;

		vm.init = function() {
			canvas().on('object:selected', function(e) {
				vm.parentVm.getActiveObjects();
			});

			canvas().on('selection:created', function(e) {
				if (e.e && e.e.shiftKey) {
					vm.parentVm.getActiveObjects();
				}
			});

			canvas().on('before:selection:cleared', function(e) {
				vm.parentVm.rightPanelOptions.panelState = false;
				vm.parentVm.rightPanelOptions.showPopupButton = true;
				$scope.$apply();
			});

			createDocumentListeners();
		};

		vm.save = saveData;

		vm.addImage = function() {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.addChosenImage;
		};

		vm.addChosenImage = function(image) {
			var imagePreviewElement = document.getElementById('image-preview');
			var sizeFromImagePreview = { height: imagePreviewElement.height, width: imagePreviewElement.width };
			vm.showFileDialog = false;
			seatMapCanvasEditSvc.addImage(canvas(), image, sizeFromImagePreview);
		};

		vm.setBackgroundImage = function() {
			vm.showFileDialog = true;
			vm.fileCallbackFunction = vm.setChosenBackgroundImage;
		};

		vm.clearBackgroundImage = function() {
			vm.setChosenBackgroundImage(null);
		};

		vm.setChosenBackgroundImage = function(image) {
			var imagePreviewElement = document.getElementById('image-preview');
			vm.showFileDialog = false;
			seatMapCanvasEditSvc.setBackgroundImage(canvas(), image, imagePreviewElement);
		};

		vm.addLocation = function() {
			vm.newLocationName = '';
			vm.showLocationDialog = true;
		};

		vm.addNamedLocation = function() {
			vm.showLocationDialog = false;
			seatMapCanvasEditSvc.addLocation(canvas(), vm.newLocationName);
		};

		vm.addSeat = function() {
			seatMapCanvasEditSvc.addSeat(canvas(), false, afterAddSeat);
		};

		vm.addText = function() {
			seatMapCanvasEditSvc.addText(canvas(), $translate.instant('DoubleClickHereToEditText'));
		};

		vm.copy = function() {
			vm.hasCopiedObjectBool = true;
			seatMapCanvasEditSvc.copy(canvas());
		};

		vm.paste = function() {
			seatMapCanvasEditSvc.paste(canvas(), afterPaste);
		};

		vm.cut = function() {
			seatMapCanvasEditSvc.copy(canvas());
			vm.delete();
		};

		vm.group = function() {
			seatMapCanvasEditSvc.group(canvas());
		};

		vm.ungroup = function() {
			seatMapCanvasEditSvc.ungroup(canvas());
		};

		vm.sendToBack = function() {
			seatMapCanvasEditSvc.sendToBack(canvas());
		};

		vm.sendBackward = function() {
			seatMapCanvasEditSvc.sendBackward(canvas());
		};

		vm.bringForward = function() {
			seatMapCanvasEditSvc.bringForward(canvas());
		};

		vm.bringToFront = function() {
			seatMapCanvasEditSvc.bringToFront(canvas());
		};

		vm.alignLeft = function() {
			seatMapCanvasEditSvc.alignLeft(canvas());
		};

		vm.alignRight = function() {
			seatMapCanvasEditSvc.alignRight(canvas());
		};

		vm.alignTop = function() {
			seatMapCanvasEditSvc.alignTop(canvas());
		};

		vm.alignBottom = function() {
			seatMapCanvasEditSvc.alignBottom(canvas());
		};

		vm.rotate45 = function() {
			seatMapCanvasEditSvc.rotate45(canvas());
		};

		vm.spaceActiveGroupVertical = function() {
			seatMapCanvasEditSvc.spaceActiveGroupVertical(canvas());
		};

		vm.spaceActiveGroupHorizontal = function() {
			seatMapCanvasEditSvc.spaceActiveGroupHorizontal(canvas());
		};

		vm.delete = function() {
			seatMapCanvasEditSvc.remove(canvas());
			updateSeatObjects();
		};

		vm.flip = function(horizontal) {
			seatMapCanvasEditSvc.flip(canvas(), horizontal);
		};

		vm.hasObjectSelected = function() {
			return (canvas().getActiveObject() || canvas().getActiveGroup()) != null;
		};

		vm.hasChanges = function() {
			var seatPropertiesChanged = vm.parentVm.loadedSeatsData != angular.toJson(vm.parentVm.seats);
			return (
				((vm.parentVm.loadedJsonData != angular.toJson(canvas()) || seatPropertiesChanged) &&
					(angular.isUndefined(vm.parentVm.loadedJsonData) ? canvas()._objects.length != 0 : true)) ||
				vm.parentVm.prefixOrSuffixChanged
			);
		};

		vm.refreshSeatMap = function() {
			vm.parentVm.refreshSeatMap();
		};

		function canvas() {
			return vm.parentVm.getCanvas();
		}

		function updateSeatObjects() {
			var removedSeatIds = seatMapCanvasUtils.getSeatsNotInArray(canvas(), vm.parentVm.seats);

			seatMapCanvasUtils.updateSeatNumberOnDelete(canvas(), removedSeatIds, vm.parentVm.seats);

			vm.parentVm.seats = vm.parentVm.seats.filter(function(seat) {
				return removedSeatIds.indexOf(seat.Id) === -1;
			});
		}

		function afterAddSeat(newSeat) {
			vm.parentVm.seats.push(newSeat);
			createDocumentListeners();
		}

		function afterPaste(seats) {
			vm.parentVm.seats = vm.parentVm.seats.concat(seats);
			createDocumentListeners();
		}

		function createDocumentListeners() {
			document.onkeydown = onKeyDownHandler;
		}

		function suspendDocumentListeners() {
			document.onkeydown = null;
		}

		function onKeyDownHandler(event) {
			performKeyDownAction(canvas(), event);
		}

		function preventDefaultEvent(event) {
			// ie <11 doesnt have e.preventDefault();
			if (event.preventDefault) event.preventDefault();
			event.returnValue = false;
		}

		function performKeyDownAction(canvas, event) {
			if (vm.parentVm.isLoading) return;
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 45: // insert
					suspendDocumentListeners();
					vm.addSeat();
					break;
				case 46: // delete
					if (event.target.nodeName.toLowerCase() !== 'input') {
						vm.delete();
					}
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
					if (event.target.nodeName.toLowerCase() !== 'input') {
						if (event.ctrlKey) {
							preventDefaultEvent(event);

							vm.cut();
						}
					}

					break;
				default:
					break;
			}
		}

		function saveData() {
			if (!vm.hasChanges()) return;

			vm.parentVm.isLoading = true;

			updateSeatObjects();

			var data = {
				SeatMapData: angular.toJson(canvas()),
				Id: vm.parentVm.seatMapId,
				ChildLocations: seatMapCanvasUtils.getLocations(canvas()),
				Seats: vm.parentVm.seats,
				LocationPrefix: vm.parentVm.locationPrefix,
				LocationSuffix: vm.parentVm.locationSuffix
			};

			seatMapCanvasEditSvc.save(data, onSaveSuccess);
			vm.parentVm.rightPanelOptions.panelState = false;
			vm.parentVm.prefixSuffixPanelOptions.panelState = false;
			vm.parentVm.prefixOrSuffixChanged = false;
		}

		function onSaveSuccess() {
			//should use $translate key
			NoticeService.success('Seat map saved successfully.', 5000, false);

			vm.refreshSeatMap();
		}
	}
})();
