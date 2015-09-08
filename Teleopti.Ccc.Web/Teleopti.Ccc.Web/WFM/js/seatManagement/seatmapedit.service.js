'use strict';

(function () {

angular.module('wfm.seatMap')
	.factory('seatMapCanvasEditService', ['seatMapCanvasUtilsService', 'seatMapService', function (utils, seatMapService) {

		var editService = {
			addImage: addImage,
			setBackgroundImage: setBackgroundImage,
			addSeat: addSeat,
			remove: remove,
			addText: addText,
			flip: flip,
			addLocation:addLocation,
			onKeyDownHandler: onKeyDownHandler,
			save: save,
			group: groupActiveObjects,
			ungroup: ungroupActiveObjects,
			sendToBack: sendToBack,
			sendBackward: sendBackward,
			bringForward: bringForward,
			bringToFront: bringToFront,
			alignLeft: alignLeft,
			alignRight: alignRight,
			alignTop: alignTop,
			alignBottom: alignBottom,
			rotate45: rotate45,
			spaceActiveGroupVertical: spaceActiveGroupVertical,
			spaceActiveGroupHorizontal: spaceActiveGroupHorizontal
		};

		function onKeyDownHandler(canvas, event) {
			//event.preventDefault();
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 67: // Ctrl+C
					if (event.ctrlKey) {
						preventDefaultEvent(event);
						copy(canvas);
					}
					break;
				case 86: // Ctrl+V
					if (event.ctrlKey) {
						preventDefaultEvent(event);
						paste(canvas);
					}
					break;
				case 45: // insert
					addSeat(canvas, false);
					break;
				case 46: // delete
					remove(canvas);
					break;
				default:
					break;
			}
		};

		function preventDefaultEvent(event) {
			// ie <11 doesnt have e.preventDefault();
			if (event.preventDefault) event.preventDefault();
			event.returnValue = false;
		};

		function copy(canvas) {
			if (utils.hasActiveGroup(canvas)) {
				// selected separate objects
				editService.copiedGroup = canvas.getActiveGroup();
			} else {
				// individual object OR individual group
				editService.copiedObject = canvas.getActiveObject();
				editService.copiedGroup = null;
			};
		};

		function paste(canvas) {
			if (editService.copiedGroup) {
				cloneSelectedItems(canvas, editService.copiedGroup);
			} else if (editService.copiedObject) {
				cloneObject(canvas, editService.copiedObject);
			}
		};

		function remove(canvas) {
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

			canvas.renderAll();
		};

		function removeObjectAndTidyReferences(canvas, obj) {

			if (obj.get('type') == 'group') {
				var objectsInGroup = obj.getObjects();
				objectsInGroup.forEach(function (child) {
					removeObjectAndTidyReferences(canvas, child);
					canvas.remove(child);
				});
			}

		};

		function cloneObject(canvas, objectToClone) {

			objectToClone.clone(function (obj) {
				pasteObject(canvas, obj);
				canvas.setActiveObject(obj);
			});
		};

		function cloneSelectedItems(canvas, cloneGroup) {

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

		function afterCloneOfSelectedItems(canvas, childObjects) {
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

		function updateSeatDataOnPaste(obj, seatPriority) {
			if (obj.type == 'group') {
				for (var i = 0; i < obj._objects.length; i++) {
					var childObj = obj._objects[i];
					seatPriority = updateSeatDataOnPaste(childObj, seatPriority);
				}

			} else {
				if (obj.type == 'seat') {
					seatPriority++;
					obj.isNew = true;
					obj.set('priority', seatPriority);
					obj.set('name', seatPriority);
					obj.set('id', getTemporaryId());
				}
			}

			return seatPriority;
		}

		function getTemporaryId() {

			function s4() {
				return Math.floor((1 + Math.random()) * 0x10000)
					.toString(16)
					.substring(1);
			}
			return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
				s4() + '-' + s4() + s4() + s4();
		};

		function addImage(canvas, image, sizeFromImagePreview) {

			fabric.Image.fromObject(image, function(img) {
				img.set({
					left: 250,
					top: 250,
					height: sizeFromImagePreview.height,
					width: sizeFromImagePreview.width
				});
				canvas.add(img);
				img.setCoords();
				canvas.renderAll();
			});
		};

		function setBackgroundImage(canvas, image, imagePreviewElement) {

			if (image == null) {
				canvas.backgroundImage = 0;
				canvas.renderAll();
				return;
			}

			fabric.Image.fromObject(image, function (img) {
				img.set({
					height: imagePreviewElement.naturalHeight,
					width: imagePreviewElement.naturalWidth
				});

				utils.scaleImage(canvas, img);
				canvas.setBackgroundImage(img);
				canvas.centerObject(img);
			});
		};

		function addSeat(canvas, withDesk) {

			var seatPriority = utils.getHighestSeatPriority(canvas) + 1;

			var imgName = 'js/SeatManagement/Images/';
			if (withDesk) {
				imgName += 'seatAndDesk.svg';
			} else {
				imgName += 'seat.svg';
			}

			var seatObj = {
				name: seatPriority.toString(),
				priority: seatPriority,
				id: getTemporaryId()
			};

			fabric.loadSVGFromURL(imgName, function (objects, options) {
				var groupedSVGObj = fabric.util.groupSVGElements(objects, options);

				fabric.util.loadImage(imgName, function (img) {

					var newSeat = new fabric.Seat(img, seatObj);
					newSeat.set({
						isNew: true,
						height: groupedSVGObj.height,
						width: groupedSVGObj.width
					});

					canvas.add(newSeat);
					newSeat.center();
					newSeat.setCoords();
				});

			});
		};

		function addLocation(canvas, name) {
			var locationObj = {
				name: name,
				id: getTemporaryId(),
				isNew: true,
				height: 100,
				width: 300,
				fill: 'rgba(59, 111, 170, 0.2)'
			};

			var newLocation = new fabric.Location(locationObj);
			canvas.add(newLocation);
			newLocation.center();
			newLocation.setCoords();
		};

		function addText(canvas, text) {
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
			canvas.renderAll();
		};


		// Layers

		function sendBackward(canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.sendBackwards(obj); });
		};

		function sendToBack(canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.sendToBack(obj); });
		};

		function bringForward(canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.bringForward(obj); });
		};

		function bringToFront(canvas) {
			actionOnActiveObject(canvas, function (obj) { canvas.bringToFront(obj); });
		};

		function actionOnActiveObject(canvas, func) {
			var activeObject = canvas.getActiveObject();
			if (activeObject) {
				func(activeObject);
			}
		};

		//Grouping

		function groupActiveObjects(canvas) {
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
			}
		};

		function ungroupActiveObjects(canvas) {
			var activeObject = canvas.getActiveObject();
			if (activeObject && activeObject._objects) {
				utils.ungroupObjects(canvas, activeObject);
			}
		};
		
		//Alignment, Spacing Rotation and Flip

		function alignLeft(canvas) {
			alignHorizontal(canvas, true);
		};

		function alignRight(canvas) {
			alignHorizontal(canvas, false);
		};

		function alignHorizontal(canvas, leftAlign) {
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

		function alignTop(canvas) {
			alignVertical(canvas, true);
		};

		function alignBottom(canvas) {
			alignVertical(canvas, false);
		};

		function alignVertical(canvas, topAlign) {
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

		function rotate45(canvas) {
			if (canvas.getActiveGroup()) {
				canvas.getActiveGroup().forEachObject(function (o) { rotateObject(canvas, o, 45); });
			} else {
				rotateObject(canvas, canvas.getActiveObject(), 45);
			}
		};

		 var rotateObject = function(canvas, obj, angleOffset) {
			var resetOrigin = false;

			if (!obj) return;

			var angle = obj.getAngle() + angleOffset;

			if ((obj.originX !== 'center' || obj.originY !== 'center') && obj.centeredRotation) {
				obj.setOriginToCenter && obj.setOriginToCenter();
				resetOrigin = true;
			}

			angle = angle > 360 ? 45 : angle < 0 ? 325 : angle;

			obj.setAngle(angle).setCoords();

			if (resetOrigin) {
				obj.setCenterToOrigin && obj.setCenterToOrigin();
			}

			canvas.renderAll();

		 };

		 function spaceActiveGroupHorizontal (canvas) {
		 	if (canvas.getActiveGroup()) {
		 		spaceGroupEvenlyHorizontal(canvas, canvas.getActiveGroup().objects);
		 	}
		 }

		 var spaceGroupEvenlyHorizontal = function (canvas, objects) {
		 	if (objects) {
		 		var left = 0;
		 		var offset = 20;
		 		var maxRange = objects.length - 1;
		 		for (var i = maxRange; i > -1; i--) {
		 			var o = objects[i];
		 			if (i == maxRange) {
		 				left = o.left;
		 			} else {
		 				left += o.width + offset;
		 				o.setLeft(left);
		 			}
		 		};
		 		canvas.renderAll();
		 	}
		 };

		 function spaceActiveGroupVertical (canvas) {
		 	if (canvas.getActiveGroup()) {
		 		spaceGroupEvenlyVertical(canvas, canvas.getActiveGroup().objects);
		 	}
		 };

		 var spaceGroupEvenlyVertical = function (canvas, objects) {
		 	if (objects) {
		 		var top = 0;
		 		var offset = 20;
		 		var maxRange = objects.length - 1;
		 		for (var i = maxRange; i > -1; i--) {
		 			var o = objects[i];
		 			if (i == maxRange) {
		 				top = o.top;
		 			} else {
		 				top += o.height + offset;
		 				o.setTop(top);
		 			}
		 		};
		 		canvas.renderAll();
		 	}
		 };

		 function flip(canvas, horizontal) {

		 	var flipCommand = horizontal ? 'flipX' : 'flipY';

		 	if (canvas.getActiveGroup()) {
		 		canvas.getActiveGroup().toggle(flipCommand);
		 	} else {
		 		canvas.getActiveObject().toggle(flipCommand);
		 	}
		 	canvas.renderAll();
		 };

		 function save(saveCommand, saveCallback) {
			seatMapService.seatMap.save(saveCommand).$promise.then(function (data) {
				saveCallback(data);
			});
		 };

		 return editService;


	}]);

}());
