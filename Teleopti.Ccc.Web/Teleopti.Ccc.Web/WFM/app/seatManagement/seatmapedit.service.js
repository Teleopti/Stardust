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
			copy: copy,
			paste: paste,
			flip: flip,
			addLocation:addLocation,
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

		function paste(canvas, onCloneSuccess) {
			if (editService.copiedGroup) {
				cloneSelectedItems(canvas, editService.copiedGroup, onCloneSuccess);
			} else if (editService.copiedObject) {
				cloneObject(canvas, editService.copiedObject, onCloneSuccess);
			}
		};

		function remove(canvas) {
			var activeObject = canvas.getActiveObject(),
				activeGroup = canvas.getActiveGroup();
			if (activeGroup) {
				canvas.discardActiveGroup();
				removeObjectAndTidyReferences(canvas, activeGroup);
			}else if (activeObject) {
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
				return;
			}
		};

		function cloneObject(canvas, objectToClone, onCloneSuccess) {

			objectToClone.clone(function (obj) {
				var seatObjects = pasteObject(canvas, obj);
				onCloneSuccess(seatObjects);
				canvas.setActiveObject(obj);
			});
		};

		function cloneSelectedItems(canvas, cloneGroup, onCloneSuccess) {
			canvas.deactivateAll();
			var childObjects = [];
			var count = cloneGroup._objects.length;
			var cloneCount = 0;

			for (var i in cloneGroup._objects) {
				cloneGroup._objects[i].clone(function (obj) {

					var seatObjects = pasteObject(canvas, obj);
					onCloneSuccess(seatObjects);

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

		function pasteObject(canvas, obj) {
			var seatObjects = [];
			updateSeatDataOnPaste(obj, utils.getHighestSeatPriority(canvas), seatObjects);
			obj.set("top", obj.top + 35);
			obj.set("left", obj.left + 35);
			canvas.add(obj);
			return seatObjects;
		};

		function updateSeatDataOnPaste(obj, seatPriority, seatObjects) {
			if (obj.type == 'group') {
				for (var i = 0; i < obj._objects.length; i++) {
					var childObj = obj._objects[i];
					seatPriority = updateSeatDataOnPaste(childObj, seatPriority, seatObjects);
				}

			} else {
				if (obj.type == 'seat') {
					seatPriority++;
					obj.isNew = true;
					obj.set('priority', seatPriority);
					obj.set('name', seatPriority);
					obj.set('id', getTemporaryId());
					seatObjects.push({ Id: obj.id, isNew: true, Priority: obj.priority, Name: obj.name, RoleIdList: [] });
				}
			}

			return seatPriority;
		};

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

		function addSeat(canvas, withDesk, addSeatSuccess) {

			var seatPriority = utils.getHighestSeatPriority(canvas) + 1;

			var imgName = 'dist/ng2/assets/seatMap/';
			if (withDesk) {
				imgName += 'seatAndDesk.svg';
			} else {
				imgName += 'seat.svg';
			}

			var seatObj = {
				id: getTemporaryId(),
				name: seatPriority.toString(),
				priority: seatPriority,
				isNew: true,
				roleIds :[]
			};

			fabric.loadSVGFromURL(imgName, function (objects, options) {
				var groupedSVGObj = fabric.util.groupSVGElements(objects, options);

				fabric.util.loadImage(imgName, function (img) {
					var newSeat = new fabric.Seat(img, seatObj);
					newSeat.set({
						height: groupedSVGObj.height,
						width: groupedSVGObj.width
					});

					canvas.add(newSeat);
					newSeat.center();
					newSeat.setCoords();
					addSeatSuccess({
						Id: seatObj.id,
						Name: seatObj.name,
						Priority: seatObj.priority,
						IsNew: (seatObj.isNew === undefined) ? false : true,
						RoleIdList: seatObj.roleIds
					});
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
				var group = new fabric.Group(objectsInGroup, { left: activeGroup.left, top: activeGroup.top});
				canvas.setActiveObject(group);
				canvas.add(group);

				canvas.renderAll();
			}
		};

		function ungroupActiveObjects(canvas) {
			var activeGroup = canvas.getActiveGroup();

			if (activeGroup) {
				var objectsInGroup = activeGroup.getObjects();
				canvas.discardActiveGroup();
				objectsInGroup.forEach(function (object) {
					utils.ungroupObjects(canvas, object);
				});
			} else {
				var activeObject = canvas.getActiveObject();
				if (activeObject && activeObject._objects) {
					utils.ungroupObjects(canvas, activeObject);
				}
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
		 		spaceGroupEvenlyHorizontal(canvas, canvas.getActiveGroup()._objects);
		 	}
		 }

		 var spaceGroupEvenlyHorizontal = function (canvas, objects) {
		 	if (objects) {
		 		var left = 0;
		 		var offset = 20;
		 		var maxRange = objects.length;
				objects.sort(function(seatA, seatB) { return seatA.priority - seatB.priority });

		 		for (var i = 0; i < maxRange; i ++) {
		 			var o = objects[i];
		 			if (i == 0) {
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
		 		spaceGroupEvenlyVertical(canvas, canvas.getActiveGroup()._objects);
		 	}
		 };

		 var spaceGroupEvenlyVertical = function (canvas, objects) {
		 	if (objects) {
		 		var top = 0;
		 		var offset = 20;
		 		var maxRange = objects.length;
		 		objects.sort(function (seatA, seatB) { return seatA.priority - seatB.priority });
		 		for (var i = 0; i < maxRange; i++) {
		 			var o = objects[i];
		 			if (i == 0) {
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
