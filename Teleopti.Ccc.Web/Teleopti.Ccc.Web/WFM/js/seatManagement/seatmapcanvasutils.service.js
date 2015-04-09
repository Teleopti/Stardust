
angular.module('wfm.seatMap')
	.factory('seatMapCanvasUtilsService', ['seatMapService', function (seatMapService) {

		var utils = {};
		utils.setupCanvas = function (canvas) {
			canvas.isGrabMode = false;
			canvas.renderOnAddRemove = true;
			canvas.stateful = false;
		}

		utils.resize = function (canvas, toolbarVisible) {

			//Robtodo: revisit, fabric canvas wrappers are a pain!
			var resizedAncestor = $('[ui-view]')[0];
			var canvasParent = $('#c');
			var top = canvasParent.offset().top;
			var left = canvasParent.position().left;
			var padding = 30;
			var heightAddition = 0;
			if (!toolbarVisible) {
				heightAddition += $('#seatMapToolbar')[0].clientHeight + padding;

			}
			canvas.setHeight((resizedAncestor.clientHeight - top) + heightAddition);
			canvas.setWidth((resizedAncestor.clientWidth - left)-padding);
		};

		utils.toggleMoveMode = function (canvas) {
			canvas.isGrabMode = !canvas.isGrabMode;
			canvas.isGrabMode ? $("canvas").css("cursor", "move") : $("canvas").css("cursor", "pointer");
		};

		utils.hasActiveGroup = function (canvas) {
			if (canvas) {
				return canvas.getActiveGroup() != null;
			}
			return false;
		};

		utils.scaleImage = function (canvas, image) {

			var ratio = canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});
		};

		utils.drawGrid = function (canvas) {
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

		utils.setSelectionMode = function (canvas, allowSelection) {
			var canvasObjects = canvas.getObjects();
			canvas.selection = allowSelection;
			for (var idx in canvasObjects) {

				canvasObjects[idx].selectable = allowSelection;
				canvasObjects[idx].hasControls = allowSelection;
				canvasObjects[idx].hasRotatingPoint = allowSelection;
				canvasObjects[idx].hasBorders = allowSelection;
			}
		};

		utils.getObjectsByType = function (canvas, type) {
			var canvasObjects = canvas.getObjects();
			var objectsArray = new Array();

			for (var obj in canvasObjects) {
				var foundObjs = utils.getObjectsOfTypeFromCanvasObject(canvasObjects[obj], type);
				if (foundObjs != null) {
					for (var idx in foundObjs) {
						objectsArray.push(foundObjs[idx]);
					}
				}
			}
			return objectsArray;
		};

		utils.getObjectsOfTypeFromCanvasObject = function (obj, type) {

			if (!obj) {
				return null;
			}
			if (obj.get('type') == type) {
				return [obj];
			}
			if (obj.get('type') == 'group') {
				return utils.getObjectsOfTypeFromGroup(obj, type);
			}

			return null;
		};

		utils.getObjectsOfTypeFromGroup = function (group, type) {
			var groupObjects = group.getObjects();
			var objs = [];
			for (var i in groupObjects) {
				if (groupObjects[i].get('type') == type) {
					objs.push(groupObjects[i]);
				} else {
					if (groupObjects[i].get('type') == 'group') {
						var subObjs = utils.getObjectsOfTypeFromGroup(groupObjects[i], type);
						for (var idx in subObjs) {
							objs.push(subObjs[idx]);
						}
					}
				}
			}
			return objs;
		}

		utils.getFirstObjectOfTypeFromCanvasObject = function (obj, type) {

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

		utils.getHighestSeatPriority = function (canvas) {

			var seatObjects = utils.getObjectsByType(canvas, 'seat');
			var highestPriority = 0;
			for (var i in seatObjects) {
				var seat = seatObjects[i];
				if (seat.priority > highestPriority) {
					highestPriority = seat.priority;
				}
			}
			return highestPriority;
		};

		utils.clearCanvas = function (canvas) {
			if (canvas != null) {
				canvas.setBackgroundImage(null);
				canvas.clear();
			}
		}

		utils.loadSeatMap = function (id, canvas, allowEdit, callbackSuccess, callbackNoJson) {
			utils.clearCanvas(canvas);
			seatMapService.seatMap.get({ id: id }).$promise.then(function (data) {
				loadSeatMapData(canvas, data, allowEdit, callbackSuccess, callbackNoJson);
			});
		};

		utils.resetZoom = function(canvas) {
			canvas.setZoom();
		};

		utils.zoom = function(canvas, value) {
			canvas.setZoom(value);
		};

		utils.scrollZooming = function($window,canvas, isScrollListening, data) {
			var e = $window.event;

			if (isScrollListening) {
				e.preventDefault();
				if (e.wheelDelta == 120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 1.1 : data.zoomValue + data.step)));
				if (e.wheelDelta == -120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 0.9 : data.zoomValue - data.step)));

				utils.zoom(canvas,data.zoomValue);
			}
			
			return data.zoomValue;
		};

		function loadSeatMapData (canvas, data, allowEdit, callbackSuccess, callbackNoJson) {

			if (data == null) {
				callbackNoJson(null);
				return;
			}
			
			var seatPriority = 0;
			var json = data.SeatMapJsonData;
			
			if (hasNoObjects(json)) {
				data.seatPriority = seatPriority;
				callbackNoJson(data);
				return;
			}

			canvas.loadFromJSON(json, function () {
				canvas.renderAll();
				var allSeats = utils.getObjectsByType(canvas, 'seat');
				for (var loadedSeat in allSeats) {
					if (allSeats[loadedSeat].priority > seatPriority) {
						seatPriority = allSeats[loadedSeat].priority;
					}
				}
				utils.setSelectionMode(canvas, allowEdit);
				data.seatPriority = seatPriority;
				callbackSuccess(data);
			});
		}

		function hasNoObjects(json) {

			if (!json) {
				return true;
			}

			var parsed = JSON.parse(json);
			if (parsed != null && parsed.objects) {
				return (parsed.objects.length == 0 &&
					(!parsed.backgroundImage));
			}

			return true;

		}

		return utils;

	}]);

