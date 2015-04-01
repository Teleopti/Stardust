
angular.module('wfm.seatMap')
	.factory('seatMapCanvasUtilsService', [function () {

		var seatMapCanvasUtilsService = {};

		seatMapCanvasUtilsService.setupCanvas=function(canvas) {
			canvas.isGrabMode = false;
			//Performance related toggles
			//self.canvas.skipTargetFind = true;
			canvas.renderOnAddRemove = true;
			canvas.stateful = false;
		}

		seatMapCanvasUtilsService.resize = function (canvas) {
			var container = canvas.wrapperEl.parentElement;
			canvas.setHeight($(container).height());
			canvas.setWidth(container.clientWidth);
		};

		seatMapCanvasUtilsService.toggleMoveMode = function (canvas) {
			canvas.isGrabMode = !canvas.isGrabMode;
			canvas.isGrabMode ? $("canvas").css("cursor", "move") : $("canvas").css("cursor", "pointer");
		};

		seatMapCanvasUtilsService.hasActiveGroup = function (canvas) {
			if (canvas) {
				return canvas.getActiveGroup() != null;
			}
			return false;
		};
	
		seatMapCanvasUtilsService.ScaleImage = function (canvas, image) {

			var ratio = canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});

		};

		seatMapCanvasUtilsService.DrawGrid = function (canvas) {
			var grid = 30;

			for (var i = 0; i < (canvas.width / grid) ; i++) {
				canvas.add(new fabric.Line([i * grid, 0, i * grid, canvas.width], { stroke: '#ccc', selectable: false }));
				canvas.add(new fabric.Line([0, i * grid, canvas.width, i * grid], { stroke: '#ccc', selectable: false }));
			}

			canvas.on('object:moving', function (options) {
				options.target.set({
					left: Math.round(options.target.left / grid) * grid,
					top: Math.round(options.target.top / grid) * grid
				});
			});


			canvas.on('object:scaling', function (options) {
				options.target.set({
					left: Math.round(options.target.left / grid) * grid,
					top: Math.round(options.target.top / grid) * grid
				});
			});

		};

		seatMapCanvasUtilsService.setSelectionMode = function (canvas, allowSelection) {
			var canvasObjects = canvas.getObjects();
			canvas.selection = allowSelection;
			for (var idx in canvasObjects) {

				canvasObjects[idx].selectable = allowSelection;
				canvasObjects[idx].hasControls = allowSelection;
				canvasObjects[idx].hasRotatingPoint = allowSelection;
				canvasObjects[idx].hasBorders = allowSelection;

			}

		};

		seatMapCanvasUtilsService.GetObjectsByType = function (canvas, type) {
			var canvasObjects = canvas.getObjects();
			var objectsArray = new Array();

			for (obj in canvasObjects) {
				var foundObjs = self.GetObjectsOfTypeFromCanvasObject(canvasObjects[obj], type);
				if (foundObjs != null) {
					for (var idx in foundObjs) {
						objectsArray.push(foundObjs[idx]);
					}
				}
			}
			return objectsArray;
		};

		seatMapCanvasUtilsService.GetObjectsOfTypeFromCanvasObject = function (obj, type) {

			if (!obj) {
				return null;
			}
			if (obj.get('type') == type) {
				return [obj];
			}
			if (obj.get('type') == 'group') {
				return self.GetObjectsOfTypeFromGroup(obj, type);
			}

			return null;
		};

		seatMapCanvasUtilsService.GetObjectsOfTypeFromGroup = function (group, type) {

			var groupObjects = group.getObjects();
			var objs = [];
			for (var i in groupObjects) {
				if (groupObjects[i].get('type') == type) {
					objs.push(groupObjects[i]);
				} else {
					if (groupObjects[i].get('type') == 'group') {
						var subObjs = self.GetObjectsOfTypeFromGroup(groupObjects[i], type);
						for (var idx in subObjs) {
							objs.push(subObjs[idx]);
						}
					}
				}
			}
			return objs;
		}

		seatMapCanvasUtilsService.GetFirstObjectOfTypeFromCanvasObject = function (obj, type) {

			if (!obj) {
				return null;
			}
			if (obj.get('type') == type) {
				return obj;
			}
			if (obj.get('type') == 'group') {
				var groupObjects = obj.getObjects();

				for (var groupObj in groupObjects) {
					if (groupObjects[groupObj].get('type') == type) {
						return groupObjects[groupObj];
					}
				}
			}

			return null;
		};

		seatMapCanvasUtilsService.ClearCanvas = function (canvas) {
			if (canvas != null) {
				canvas.setBackgroundImage(null);
				canvas.clear();
			}
		}

		seatMapCanvasUtilsService.LoadSeatMap = function (canvas, data, allowSelection, callback) {

			self.ClearCanvas(canvas);

			var seatPriority = 0;
			var json = data.SeatMapJsonData;
			canvas.loadFromJSON(json, function () {
				canvas.renderAll();
				var allSeats = self.GetObjectsByType(canvas, 'seat');
				for (var loadedSeat in allSeats) {
					if (allSeats[loadedSeat].priority > seatPriority) {
						seatPriority = allSeats[loadedSeat].priority;
					}
				}
				self.SetSelectionMode(canvas, allowSelection);
				data.seatPriority = seatPriority;
				callback(data);
			});

		};

		return seatMapCanvasUtilsService;

	}]);

