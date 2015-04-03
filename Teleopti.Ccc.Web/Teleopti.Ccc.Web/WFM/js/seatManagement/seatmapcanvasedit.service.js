'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapCanvasEditService', ['seatMapCanvasUtilsService', 'seatMapService', function (utils, seatMapService) {


		var onKeyDownHandler = function (canvas, event) {
			//event.preventDefault();
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 67: // Ctrl+C
					if (event.ctrlKey) {
						event.preventDefault();
						copy(canvas);
					}
					break;
				case 86: // Ctrl+V
					if (event.ctrlKey) {
						event.preventDefault();
						paste(canvas);
					}
					break;
				case 46: // delete
					remove(canvas);
					break;
				default:
					break;
			}
		};

		var copy = function (canvas) {
			if (utils.hasActiveGroup(canvas)) {
				// selected separate objects
				editService.copiedGroup = canvas.getActiveGroup();
			} else {
				// individual object OR individual group
				editService.copiedObject = canvas.getActiveObject();
				editService.copiedGroup = null;
			};
		};

		var paste = function (canvas) {
			if (editService.copiedGroup) {
				cloneSelectedItems(canvas, editService.copiedGroup);
			} else if (editService.copiedObject) {
				cloneObject(canvas, editService.copiedObject);
			}
		};

		var remove = function (canvas) {
			var activeObject = canvas.getActiveObject(),
				activeGroup = canvas.getActiveGroup();

			if (activeGroup) {
				canvas.discardActiveGroup();
				removeObjectAndTidyReferences(canvas, activeGroup);
			}
			else if (activeObject) {
				canvas.remove(activeObject);
				removeObjectAndTidyReferences(canvas, activeObject);
			}
		};

		var removeObjectAndTidyReferences = function (canvas, obj) {

			if (obj.get('type') == 'group') {
				var objectsInGroup = obj.getObjects();
				objectsInGroup.forEach(function (child) {
					removeObjectAndTidyReferences(canvas, child);
					canvas.remove(child);
				});
			}

			//canvas.renderAll();
		};

		var cloneObject = function (canvas, objectToClone) {

			objectToClone.clone(function (obj) {
				pasteObject(canvas, obj);
				canvas.setActiveObject(obj);
			});
		};

		var cloneSelectedItems = function (canvas, cloneGroup) {

			canvas.deactivateAll();

			var childObjects = [];
			var count = cloneGroup._objects.length;
			var cloneCount = 0;

			for (var i in cloneGroup._objects) {
				cloneGroup._objects[i].clone(function (obj) {
					pasteObject(canvas, obj);
					childObjects.push(obj);
					cloneCount++;
					if (cloneCount == count) {
						afterCloneOfSelectedItems(canvas, childObjects);
					}
				});
			}
		};

		var afterCloneOfSelectedItems = function (canvas, childObjects) {

			var group = new fabric.Group(childObjects.reverse(), {
				canvas: canvas
			});
			group.addWithUpdate();
			canvas.setActiveGroup(group, childObjects);
			group.saveCoords();
			canvas.renderAll();
		};

		var pasteObject = function (canvas, obj) {
			updateSeatDataOnPaste(obj, utils.getHighestSeatPriority(canvas));
			obj.set("top", obj.top + 35);
			obj.set("left", obj.left + 35);
			canvas.add(obj);
		};

		var updateSeatDataOnPaste = function (obj, seatPriority) {
			if (obj.type == 'group') {
				for (var i = 0; i < obj._objects.length; i++) {
					var childObj = obj._objects[i];
					updateSeatDataOnPaste(childObj, seatPriority);
				}

			} else {
				if (obj.type == 'seat') {
					seatPriority++;
					obj.isNew = true;
					obj.set('priority', seatPriority);
					obj.set('id', getTemporaryId());
					//ROBTODO!
					//self.newSeats.push(obj);
				}
			}
		}

		var getTemporaryId = function () {

			function s4() {
				return Math.floor((1 + Math.random()) * 0x10000)
					.toString(16)
					.substring(1);
			}
			return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
				s4() + '-' + s4() + s4() + s4();
		};


		var addImage = function (canvas, imageName) {
			fabric.Image.fromURL('js/SeatManagement/Images/' + imageName, function (image) {
				image.set({
					left: 400,
					top: 400
				});
				canvas.add(image);
				//canvas.renderAll();
			});
		};

		var addSeat = function (canvas, withDesk) {

			var seatPriority = utils.getHighestSeatPriority(canvas) + 1;

			var imgName = 'js/SeatManagement/Images/';
			if (withDesk) {
				imgName += 'seatAndDesk.svg';
			} else {
				imgName += 'seat.svg';
			}

			var seatObj = {
				name: 'Unnamed seat',
				priority: seatPriority,
				id: getTemporaryId()
			};

			fabric.util.loadImage(imgName, function (img) {
				var newSeat = new fabric.Seat(img, seatObj);
				newSeat.isNew = true;
				canvas.add(newSeat);
				newSeat.center();
				newSeat.setCoords();
			});
		};

		var addText = function (canvas, text) {
			var textSample = new fabric.IText(text.slice(0, text.length), {
				left: 400,
				top: 300,
				fontFamily: 'helvetica',
				angle: 0,
				fill: '#000000',
				fontSize: 18,
				fontWeight: '',
				originX: 'left',
				hasRotatingPoint: true,
				centerTransform: true
			});

			canvas.add(textSample);
			//canvas.renderAll();
		};


		// Layers

		var sendBackward = function (canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.sendBackwards(obj); });
		};

		var sendToBack = function (canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.sendToBack(obj); });
		};

		var bringForward = function (canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.bringForward(obj); });
		};

		var bringToFront = function (canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.bringToFront(obj); });
		};

		var actionOnActiveObject = function (canvas, func) {
			var activeObject = canvas.getActiveObject();
			if (activeObject) {
				func(activeObject);
			}
		};

		//Grouping

		var groupActiveObjects = function (canvas) {
			var activeGroup = canvas.getActiveGroup();
			if (activeGroup) {
				var objectsInGroup = activeGroup.getObjects();
				canvas.discardActiveGroup();
				objectsInGroup.forEach(function (object) {
					canvas.remove(object);

				});
				var group = new fabric.Group(objectsInGroup, { left: activeGroup.left + 15, top: activeGroup.top + 15 });
				canvas.setActiveObject(group);
				canvas.add(group);
				//self.canvas.renderAll();
			}
		};

		var ungroupActiveObjects = function (canvas) {
			var activeObject = canvas.getActiveObject();
			if (activeObject && activeObject._objects) {
				ungroupObjects(canvas, activeObject);
			}
		};

		var ungroupObjects = function (canvas, group) {
			var items = group._objects;
			// translate the group-relative coordinates to canvas relative ones
			group._restoreObjectsState();
			canvas.remove(group);
			for (var i = 0; i < items.length; i++) {
				canvas.add(items[i]);
			}
			//self.canvas.renderAll();
		};

		//Alignment, Spacing Rotation and Flip

		var alignLeft = function (canvas) {
			alignHorizontal(canvas, true);
		};

		var alignRight = function (canvas) {
			alignHorizontal(canvas, false);
		};

		var alignHorizontal = function (canvas, leftAlign) {
			var activeGroup = canvas.getActiveGroup();
			if (activeGroup) {
				var left = 0;
				activeGroup.forEachObject(function (o) {
					if (leftAlign) {
						if (o.left < left) {
							left = o.left;
						}
					}
					else {
						if (o.left > left) {
							left = o.left;
						}
					}
				});

				if (activeGroup.forEachObject(function (o) {
					var angle = o.angle;
					o.setAngle(0);
					o.set("left", left);
					o.setAngle(angle);
				}));

				canvas.renderAll();
			}
		}

		var alignTop = function (canvas) {
			alignVertical(canvas, true);
		};

		var alignBottom = function (canvas) {
			alignVertical(canvas, false);
		};

		var alignVertical = function (canvas, topAlign) {
			var activeGroup = canvas.getActiveGroup();
			if (activeGroup) {
				var top = 0;
				activeGroup.forEachObject(function (o) {
					if (topAlign) {
						if (o.top < top) {
							top = o.top;
						}
					}
					else {
						if (o.top > top) {
							top = o.top;
						}
					}
				});
				activeGroup.forEachObject(function (o) {
					var angle = o.angle;
					o.setAngle(0);
					o.set("top", top);
					o.setAngle(angle);
				});

				canvas.renderAll();
			}
		};
		var save = function (seatMapSaveCommand, saveCallback) {
			seatMapService.seatMap.save(seatMapSaveCommand).$promise.then(function (data) {
				saveCallback(data);
			});

		};

		var editService = {
			
			addImage : addImage,
			addSeat : addSeat,
			onKeyDownHandler : onKeyDownHandler,
			save : save,
			group : groupActiveObjects,
			ungroup : ungroupActiveObjects,
			sendToBack : sendToBack,
			sendBackward : sendBackward,
			bringForward : bringForward,
			bringToFront : bringToFront,
			alignLeft: alignLeft,
			alignRight : alignRight,
			alignTop: alignTop,
			alignBottom: alignBottom,


		};

		//editService.addImage = addImage;
		//editService.addSeat = addSeat;
		//editService.onKeyDownHandler = onKeyDownHandler;
		//editService.save = save;
		//editService.group = groupActiveObjects;
		//editService.ungroup = ungroupActiveObjects;
		//editService.sendToBack = sendToBack;
		//editService.sendBackward = sendBackward;
		//editService.bringForward = bringForward;
		//editService.bringToFront = bringToFront;
		//editService.alignLeft= alignLeft;
		//editService.alignRight = alignRight;
		//editService.alignTop= alignTop;
		//editService.alignBottom= alignBottom;

		return editService;

	}]);

