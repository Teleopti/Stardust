﻿
(function () {

angular.module('wfm.seatMap')
	.factory('seatMapCanvasUtilsService', ['seatMapService','$timeout', function (seatMapService, timeout) {

		var utils = {

			setupCanvas: setupCanvas,
			resize: resize,
			toggleMoveMode: toggleMoveMode,
			hasActiveGroup: hasActiveGroup,
			scaleImage: scaleImage,
			setSelectionMode: setSelectionMode,
			getFirstObjectOfTypeFromCanvasObject: getFirstObjectOfTypeFromCanvasObject,
			getLocations: getLocations,
			getSeats: getSeats,
			getHighestSeatPriority: getHighestSeatPriority,
			clearCanvas : clearCanvas,
			loadSeatMap: loadSeatMap,
			resetPosition: resetPosition,
			scrollZooming : scrollZooming,
			zoom: zoom,
			applyOccupancyColoring: applyOccupancyColoring,
			loadSeatDetails: loadSeatDetails
		};
		
		function setupCanvas (canvas) {
			canvas.isGrabMode = false;
			canvas.renderOnAddRemove = true;
			canvas.stateful = false;
		}

		function resize(canvas) {
			timeout(function(){ doResize(canvas) }, 0, true);
		};

		function doResize(canvas) {
			var viewPortHeight = $(document)[0].documentElement.clientHeight;
			var width = $('[ui-view]')[0].clientWidth - 20;
			var heightReduction = 130; // no reliable element to base this off
			
			canvas.setHeight((viewPortHeight - heightReduction));
			canvas.setWidth(width);
		}

		function toggleMoveMode(canvas) {
			canvas.isGrabMode = !canvas.isGrabMode;
			canvas.isGrabMode ? $("canvas").css("cursor", "move") : $("canvas").css("cursor", "pointer");
		};

		function hasActiveGroup (canvas) {
			if (canvas) {
				return canvas.getActiveGroup() != null;
			}
			return false;
		};

		function scaleImage (canvas, image) {

			var ratio = canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});
		};

		function drawGrid (canvas) {
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

		function setSelectionMode (canvas, allowSelection) {
			var canvasObjects = canvas.getObjects();
			canvas.deactivateAllWithDispatch().renderAll();
			canvas.selection = allowSelection;
			for (var idx in canvasObjects) {

				canvasObjects[idx].selectable = allowSelection;
				canvasObjects[idx].hasControls = allowSelection;
				canvasObjects[idx].hasRotatingPoint = allowSelection;
			}
		};

	
		function getObjectsByType(canvas, type) {

			var objectsArray = new Array();

			getObjectByTypeAndAddUsingFunction(canvas, type, function (obj) {
				objectsArray.push(obj);
			});
			
			return objectsArray;
		};
		

		function getObjectsByTypeDict(canvas, type) {

			var objectsDict = {};

			getObjectByTypeAndAddUsingFunction(canvas, type, function (obj) {
				objectsDict[obj.id] = obj;
			});

			return objectsDict;
		};
		
		function getObjectByTypeAndAddUsingFunction(canvas, type, addFunction) {
			var canvasObjects = canvas.getObjects();
			for (var obj in canvasObjects) {
				var foundObjs = getObjectsOfTypeFromCanvasObject(canvasObjects[obj], type);
				if (foundObjs != null) {
					for (var idx in foundObjs) {
						addFunction(foundObjs[idx]);
					}
				}
			}
		};

		function getObjectsOfTypeFromCanvasObject (obj, type) {

			if (!obj) {
				return null;
			}
			if (obj.get('type') == type) {
				return [obj];
			}
			if (obj.get('type') == 'group') {
				return getObjectsOfTypeFromGroup(obj, type);
			}

			return null;
		};

		 function getObjectsOfTypeFromGroup (group, type) {
			var groupObjects = group.getObjects();
			var objs = [];
			for (var i in groupObjects) {
				if (groupObjects[i].get('type') == type) {
					objs.push(groupObjects[i]);
				} else {
					if (groupObjects[i].get('type') == 'group') {
						var subObjs = getObjectsOfTypeFromGroup(groupObjects[i], type);
						for (var idx in subObjs) {
							objs.push(subObjs[idx]);
						}
					}
				}
			}
			return objs;
		}

		function getFirstObjectOfTypeFromCanvasObject (obj, type) {

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


		function getLocations (canvas) {
			var childLocations = [];
			var locations = getObjectsByType(canvas, 'location');
			for (var i in locations) {
				var location = locations[i];
				childLocations.push(
				{
					Id: location.id,
					Name: location.name,
					IsNew: (location.isNew === undefined) ? false : true
				});
			}
			return childLocations;
		};

		function getSeats (canvas) {
			var seats = [];
			var seatObjects = getObjectsByType(canvas, 'seat');
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


		function getHighestSeatPriority (canvas) {

			var seatObjects = getObjectsByType(canvas, 'seat');
			var highestPriority = 0;
			for (var i in seatObjects) {
				var seat = seatObjects[i];
				if (seat.priority > highestPriority) {
					highestPriority = seat.priority;
				}
			}
			return highestPriority;
		};

		function clearCanvas (canvas) {
			if (canvas != null) {
				canvas.setBackgroundImage(null);
				canvas.clear();
			}
		}

		function loadSeatMap (id, date, canvas, allowEdit, callbackSuccess, callbackNoJson) {
			clearCanvas(canvas);
			
			seatMapService.seatMap.get({ id: id, date: date }).$promise.then(function(data) {
				loadSeatMapData(canvas, data, allowEdit, callbackSuccess, callbackNoJson);
			});
			
		};

		function loadSeatDetails(seatId, selectedDate) {
			return seatMapService.occupancy.get({ seatId: seatId, date: selectedDate }).$promise;
		};

		function resetPosition (canvas) {
			canvas.resetPosition();
		};

		function zoom (canvas, value) {
			canvas.setZoom(value);
		};

		function scrollZooming ($window,canvas, isScrollListening, data) {
			var e = $window.event;

			if (isScrollListening) {
				e.preventDefault();
				if (e.wheelDelta == 120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 1.1 : data.zoomValue + data.step)));
				if (e.wheelDelta == -120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 0.9 : data.zoomValue - data.step)));

				zoom(canvas,data.zoomValue);
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
				var allSeats = getObjectsByType(canvas, 'seat');
				for (var loadedSeat in allSeats) {
					if (allSeats[loadedSeat].priority > seatPriority) {
						seatPriority = allSeats[loadedSeat].priority;
					}
				}
				setSelectionMode(canvas, allowEdit);
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
		
		function applyOccupancyColoring(canvas, seatInfo) {
		
			var occupiedSeatObjects = [];
			var seatDict = getObjectsByTypeDict(canvas, 'seat');
			var occupancyColourFilter = new fabric.Image.filters.Blend({
				color: '#C2E085',
				mode: 'multiply'
			});

			seatInfo.forEach(function (seat) {
				if (seat.IsOccupied) {
					var occupiedSeat = seatDict[seat.Id];
					occupiedSeat.filters.push(occupancyColourFilter);
					occupiedSeatObjects.push(occupiedSeat);			
				}
			});

			applyFiltersToSeats(canvas, occupiedSeatObjects);
		};
		
		function applyFiltersToSeats(canvas, occupiedSeatObjects) {
			
			var i = 0;
			function onFilterCallBack() {
				i++;
				if (i == occupiedSeatObjects.length) {
					canvas.renderAll.bind(canvas)();
				}
			};

			occupiedSeatObjects.forEach(function (seat) {
				seat.applyFilters(onFilterCallBack);
			});

		};


		return utils;

	}]);


}());
