﻿
(function () {

	angular.module('wfm.seatMap')
		.factory('seatMapCanvasUtilsService', ['seatMapService', '$timeout', function (seatMapService, timeout) {

			var utils = {

				setupCanvas: setupCanvas,
				resize: resize,
				toggleMoveMode: toggleMoveMode,
				hasActiveGroup: hasActiveGroup,
				scaleImage: scaleImage,
				setSelectionMode: setSelectionMode,
				getFirstObjectOfTypeFromCanvasObject: getFirstObjectOfTypeFromCanvasObject,
				getSeatObject: getSeatObject,
				getLocations: getLocations,
				getSeats: getSeats,
				getSeatBookingTimeDisplay: getSeatBookingTimeDisplay,
				getHighestSeatPriority: getHighestSeatPriority,
				clearCanvas: clearCanvas,
				loadSeatMap: loadSeatMap,
				resetPosition: resetPosition,
				scrollZooming: scrollZooming,
				ungroupObjects: ungroupObjects,
				zoom: zoom,
				showSeatBooking: showSeatBooking,
				loadSeatDetails: loadSeatDetails,
				ungroupObjectsSoTheyCanBeIndividuallySelected: ungroupObjectsSoTheyCanBeIndividuallySelected
			};

			function setupCanvas(canvas) {
				canvas.isGrabMode = false;
				canvas.renderOnAddRemove = true;
				canvas.stateful = false;
			}

			function resize(canvas) {
				timeout(function () { doResize(canvas) }, 50, true);
			};

			function doResize(canvas) {
				var viewPortHeight = $(document)[0].documentElement.clientHeight;
				var width = $('[ui-view]')[0].clientWidth - 0;
				var heightReduction = 130; // no reliable element to base this off

				canvas.setHeight((viewPortHeight - heightReduction));
				canvas.setWidth(width);
			}

			function toggleMoveMode(canvas) {
				canvas.isGrabMode = !canvas.isGrabMode;
				canvas.isGrabMode ? $("canvas").css("cursor", "move") : $("canvas").css("cursor", "pointer");
			};

			function hasActiveGroup(canvas) {
				if (canvas) {
					return canvas.getActiveGroup() != null;
				}
				return false;
			};

			function scaleImage(canvas, image) {

				var ratio = canvas.height / image.height;
				image.set({
					scaleY: ratio,
					scaleX: ratio
				});
			};

			function drawGrid(canvas) {
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

			function setSelectionMode(canvas, allowSelection) {
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

			function getObjectsOfTypeFromCanvasObject(obj, type) {

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

			function getObjectsOfTypeFromGroup(group, type) {
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

			function getFirstObjectOfTypeFromCanvasObject(obj, type) {

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


			function getLocations(canvas) {
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

			function createSeatFromSeatObj(seatObj) {
				return {
					Id: seatObj.id,
					Name: seatObj.name,
					Priority: seatObj.priority,
					IsNew: (seatObj.isNew === undefined) ? false : true
				};
			}


			function getSeats(canvas) {
				var seats = [];
				var seatObjects = getObjectsByType(canvas, 'seat');
				for (var i in seatObjects) {
					var seatObj = seatObjects[i];
					seats.push(createSeatFromSeatObj(seatObj));
				}
				return seats;
			};

			function getSeatObject(canvas, seatIndex, isFirstSeat) {

				var seatObjects = getObjectsByType(canvas, 'seat');
				if (seatObjects.length > 0) {

					if (isFirstSeat) {
						return seatObjects[getTopLeftSeat(canvas)];
					}else {
						return seatObjects[seatIndex];
					}
				}
				return null;
			};

			function getTopLeftSeat(canvas) {

				var seatObjects = getObjectsByType(canvas, 'seat');
				var firstSeatPositionX = seatObjects[0].left;
				var firstSeatPositionY = seatObjects[0].top;
				var firstSeatIndex = 0;
				for (var i in seatObjects) {
					var seat = seatObjects[i];
					if ((seat.left + seat.top) <= (firstSeatPositionX + firstSeatPositionY)) {
						firstSeatPositionX = seat.left;
						firstSeatPositionY = seat.top;
						firstSeatIndex = i;
					}
				}

				return firstSeatIndex;
			};

			function getHighestSeatPriority(canvas) {

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

			function clearCanvas(canvas) {
				if (canvas != null) {
					canvas.setBackgroundImage(null);
					canvas.clear();
				}
			}

			function loadSeatMap(id, date, canvas, allowEdit, callbackSuccess, callbackNoJson) {
				clearCanvas(canvas);

				seatMapService.seatMap.get({ id: id, date: date }).$promise.then(function (data) {
					loadSeatMapData(canvas, data, allowEdit, callbackSuccess, callbackNoJson);
				});

			};

			function loadSeatDetails(seatId, selectedDate) {
				return seatMapService.occupancy.get({ seatId: seatId, date: selectedDate }).$promise;
			};

			function resetPosition(canvas) {
				canvas.resetPosition();
			};

			function zoom(canvas, value) {
				canvas.setZoom(value);
			};

			function scrollZooming($window, canvas, isScrollListening, data) {
				var e = $window.event;

				if (isScrollListening) {
					e.preventDefault();
					if (e.wheelDelta == 120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 1.1 : data.zoomValue + data.step)));
					if (e.wheelDelta == -120) data.zoomValue = Math.max(data.min, Math.min(data.max, ((data.zoomValue > 1) ? data.zoomValue * 0.9 : data.zoomValue - data.step)));

					zoom(canvas, data.zoomValue);
				}

				return data.zoomValue;
			};

			function loadSeatMapData(canvas, data, allowEdit, callbackSuccess, callbackNoJson) {

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

			function showSeatBooking(canvas, seatInfo) {

				var occupiedSeatObjects = [];
				var seatDict = getObjectsByTypeDict(canvas, 'seat');

				seatInfo.forEach(function (seat) {
					if (seat.IsOccupied) {
						var occupiedSeat = seatDict[seat.Id];
						occupiedSeatObjects.push(occupiedSeat);
					}
				});

				var occupiedSeatObjectProcessedCount = 0;

				var callbackOnLoadImage = function () {
					occupiedSeatObjectProcessedCount++;
					if (occupiedSeatObjectProcessedCount == occupiedSeatObjects.length) {
						canvas.renderAll();
					};
				};

				occupiedSeatObjects.forEach(function (occupiedSeat) {
					loadSeatBookedImage(canvas, occupiedSeat, callbackOnLoadImage);
				});

			};

			function loadSeatBookedImage(canvas, occupiedSeat, callback) {
				var seatWithBookingUrl = 'js/SeatManagement/Images/seatWithBooking.svg';
				fabric.loadSVGFromURL(seatWithBookingUrl, function (objects, options) {
					var groupedSvgObj = fabric.util.groupSVGElements(objects, options);
					fabric.util.loadImage(seatWithBookingUrl, function (img) {
						occupiedSeat.setElement(img);
						occupiedSeat.set({
							height: groupedSvgObj.height,
							width: groupedSvgObj.width
						});
						callback();
					});
				});

			};

			function getTimeZoneAdjustmentDisplay(booking) {

				var belongsToDateMoment = moment(booking.BelongsToDate.Date);
				var startDateMoment = moment(booking.StartDateTime);

				var timeZoneAdjustment = "";

				if (startDateMoment.isAfter(belongsToDateMoment, 'day')) {
					timeZoneAdjustment += " (+1)";
				}
				if (startDateMoment.isBefore(belongsToDateMoment, 'day')) {
					timeZoneAdjustment += " (-1)";
				}

				return timeZoneAdjustment;
			};

			function getSeatBookingTimeDisplay(booking) {
				return moment(booking.StartDateTime).format('LT') + " - " + moment(booking.EndDateTime).format('LT') + getTimeZoneAdjustmentDisplay(booking);
			};


			function getAllGroups(canvas) {
				var canvasObjects = canvas.getObjects();
				var groups = [];
				canvasObjects.forEach(function (obj) {
					if (obj.get('type') == 'group') {
						groups.push(obj);
					}
				});
				return groups;
			};

			function ungroupObjects(canvas, group) {

				var items = group._objects;
				// translate the group-relative coordinates to canvas relative ones
				group._restoreObjectsState();
				canvas.remove(group);
				for (var i = 0; i < items.length; i++) {
					canvas.add(items[i]);
				}
			};

			function deepUnGroup(canvas, group) {
				ungroupObjects(canvas, group);
				group._objects.forEach(function (childItem) {
					if (childItem.get('type') == 'group') {
						deepUnGroup(canvas, childItem);
					};
				});
			};

			function ungroupObjectsSoTheyCanBeIndividuallySelected(canvas) {

				// grouped objects will be ungrouped so they can be selected in the seat map booking view
				// keep an eye on https://github.com/kangax/fabric.js/issues/485 as if/when this is implemented we
				// can better handle click events on grouped objects.

				getAllGroups(canvas).forEach(function (group) {
					deepUnGroup(canvas, group);
				});

				setSelectionMode(canvas, false);
			};

			return utils;
		}]);
}());
