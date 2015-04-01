'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapCanvasEditService', ['seatMapCanvasUtilsService', function (utils) {

		var editService = {};

		editService.AddImage = function (canvas, imageName) {
			fabric.Image.fromURL('../Areas/SeatPlanner/Content/Images/' + imageName, function (image) {
				image.set({
					left: 400,
					top: 400
				});
				canvas.add(image);
			});
		};


		editService.OnKeyDownHandler = function (canvas, event) {
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
					//self.RemoveSelected();
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

		var cloneObject = function (canvas, objectToClone) {

			objectToClone.clone(function (obj) {
				//Robtodo: Implement seat data paste (priority, names etc)
				//self.UpdateSeatDataOnPaste(obj);
				obj.set("top", obj.top + 35);
				obj.set("left", obj.left + 35);

				canvas.add(obj);
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
					//Robtodo: Implement seat data paste (priority, names etc)
					//self.UpdateSeatDataOnPaste(obj);
					obj.set("top", obj.top + 35);
					obj.set("left", obj.left + 35);
					canvas.add(obj);
					childObjects.push(obj);
					cloneCount++;
					if (cloneCount == count) {
						afterCloneOfSelectedItems(canvas, childObjects);
					}
				});
			}
		}

		var afterCloneOfSelectedItems = function (canvas, childObjects) {

			var group = new fabric.Group(childObjects.reverse(), {
				canvas: canvas
			});
			group.addWithUpdate();
			canvas.setActiveGroup(group, childObjects);
			group.saveCoords();
			canvas.renderAll();
		}
		

		
		
		return editService;


	}]);

