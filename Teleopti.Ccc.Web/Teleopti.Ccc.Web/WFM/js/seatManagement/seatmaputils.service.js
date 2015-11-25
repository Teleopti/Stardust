
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
				getObjectsOfTypeFromGroup: getObjectsOfTypeFromGroup,
				getSeatObjectById: getSeatObjectById,
				getSeatObject: getSeatObject,
				createSeatFromFabricSeatObj: createSeatFromFabricSeatObj,
				getObjectsByType: getObjectsByType,
				getActiveFabricObjectsByType: getActiveFabricObjectsByType,
				getLocations: getLocations,
				getActiveSeatObjects: getActiveSeatObjects,
				getSeatBookingTimeDisplay: getSeatBookingTimeDisplay,
				getHighestSeatPriority: getHighestSeatPriority,
				clearCanvas: clearCanvas,
				loadSeatMap: loadSeatMap,
				resetPosition: resetPosition,
				scrollZooming: scrollZooming,
				ungroupObjects: ungroupObjects,
				zoom: zoom,
				showSeatBooking: showSeatBooking,
				loadOccupancyDetailsForSeats: loadOccupancyDetailsForSeats,
				selectGroupOfObjects: selectGroupOfObjects,
				ungroupObjectsSoTheyCanBeIndividuallySelected: ungroupObjectsSoTheyCanBeIndividuallySelected,
				selectMultipleSeatsForScenarioTest: selectMultipleSeatsForScenarioTest,
			};

			function matchFabricSeatToSeatObjects(fabricSeats, seatObjects, activeSeats) {
				var hash = {};

				fabricSeats.forEach(function (fabricSeat) {
					hash[fabricSeat.id] = true;
				});

				seatObjects.forEach(function (seatObj) {
					if (typeof hash[seatObj.Id] !== 'undefined') {
						activeSeats.push(seatObj);
					}
				});
			};

			function getActiveSeatObjects(canvas, seatObjects, activeSeats) {
				var fabricSeats = getActiveFabricObjectsByType(canvas, 'seat');
				matchFabricSeatToSeatObjects(fabricSeats, seatObjects, activeSeats);
			};

			function setupCanvas(canvas) {
				canvas.isGrabMode = false;
				canvas.renderOnAddRemove = false;
				canvas.stateful = false;
			}

			function resize(canvas) {
				timeout(function () { doResize(canvas) }, 50, true);
			};

			function doResize(canvas) {
				var viewPortHeight = $('.seatmap').height();
				var width = $('[ui-view]')[0].clientWidth - 0;
				var heightReduction = $('#seatmap-toolbar').height() + $('.location-breadcrumb').height() + 5;

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

			function setSelectionMode(canvas, canSelect) {

				var canvasObjects = canvas.getObjects();
				canvas.deactivateAllWithDispatch().renderAll();

				if (!canSelect) {
					canvas.selection = false;
					for (var idx in canvasObjects) {
						canvasObjects[idx].selectable = false;
					}
					return;
				}

				canvas.selection = true;
				for (var idx in canvasObjects) {
					if (canvasObjects[idx].type == 'location') {
						canvasObjects[idx].selectable = false;
					} else {
						canvasObjects[idx].selectable = true;
					}
					canvasObjects[idx].hasBorders = true;
					canvasObjects[idx].lockRotation = false;
					canvasObjects[idx].lockScalingX = false;
					canvasObjects[idx].lockMovementX = true;
					canvasObjects[idx].lockMovementY = true;
					canvasObjects[idx].hasControls = false;
				}
			}

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

			function getActiveFabricObjectsByType(canvas, type) {

				var seperateObjects, bindedObject,
					activeObjects = [], result = [];

				if (hasActiveGroup(canvas)) {
					seperateObjects = canvas.getActiveGroup();
					seperateObjects._objects.forEach(function (seperateObj) {
						if (seperateObj._objects) {
							seperateObj._objects.forEach(function (groupObj) {
								activeObjects.push(groupObj);
							});
						} else {
							activeObjects.push(seperateObj);
						}
					});
				} else {
					bindedObject = canvas.getActiveObject();

					if (bindedObject._objects) {
						bindedObject._objects.forEach(function (bindedObj) {
							activeObjects.push(bindedObj);
						});
					} else {
						activeObjects.push(bindedObject);
					}
				}

				activeObjects.forEach(function (activeObj) {
					if (activeObj.type == type)
						result.push(activeObj);
				});
				return result;
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

			function createSeatFromFabricSeatObj(fabricSeatObj) {
				return {
					Id: fabricSeatObj.id,
					Name: fabricSeatObj.name,
					Priority: fabricSeatObj.priority,
					IsNew: (fabricSeatObj.isNew === undefined) ? false : true,
					RoleIdList: fabricSeatObj.roleIds
				};
			};

			function getSeatObject(canvas, selectTopLeftSeat) {

				var seatObjects = getObjectsByType(canvas, 'seat');
				if (seatObjects.length > 0) {

					if (selectTopLeftSeat) {
						return seatObjects[getTopLeftSeat(canvas)];
					} else {
						return seatObjects[0];
					}
				}
				return null;
			};

			function getSeatObjectById(canvas, id) {

				var seatObjects = getObjectsByType(canvas, 'seat');
				if (seatObjects.length > 0) {
					for (var i in seatObjects) {
						var seat = seatObjects[i];

						if (seat.id == id) {
							return seat;
						}
					};
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

			function loadSeatMap(id, date, canvas, allowEdit, canSelectObjects, callbackSuccess, callbackNoJson) {
				clearCanvas(canvas);

				seatMapService.seatMap.get({ id: id, date: date }).$promise.then(function (data) {
					loadSeatMapData(canvas, data, allowEdit, canSelectObjects, callbackSuccess, callbackNoJson);
				});
			};

			function loadOccupancyDetailsForSeats(seatIds, selectedDate) {
				var selectedDateDateOnly = moment(selectedDate).format('YYYY-MM-DD');
				return seatMapService.occupancy.get({ SeatIds: seatIds, Date: selectedDateDateOnly }).$promise;
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

			function loadSeatMapData(canvas, data, allowEdit, canSelect, callbackSuccess, callbackNoJson) {

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

				var seatsIdAndPriorityDic = {};
				data.Seats.forEach(function(seatInfo) {
					seatsIdAndPriorityDic[seatInfo.Id] = seatInfo.Priority;
				});

				canvas.loadFromJSON(json, function () {
					canvas.renderAll();
					var allSeats = getObjectsByType(canvas, 'seat');
					for (var loadedSeat in allSeats) {

						allSeats[loadedSeat].priority = seatsIdAndPriorityDic[allSeats[loadedSeat].id];

						if (allSeats[loadedSeat].priority > seatPriority) {
							seatPriority = allSeats[loadedSeat].priority;
						}
					}
					canvas.renderAll();
					if (!allowEdit)
						setSelectionMode(canvas, canSelect);
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

				canvas.discardActiveObject();

				for (var i = 0; i < items.length; i++) {
					canvas.add(items[i]);
				}

				canvas.renderAll();

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

				setSelectionMode(canvas, true);
			};


			function selectGroupOfObjects(canvas, objects) {

				var objs = objects.map(function (o) {
					return o.set('active', true);
				});

				var group = new fabric.Group(objs, {
					originX: 'center',
					originY: 'center'
				});

				canvas._activeObject = null;

				canvas.setActiveGroup(group.setCoords()).renderAll();
			}


			function selectMultipleSeatsForScenarioTest(canvas, seatNumber) {
				var seatmapOccupancyScope = angular.element(document.getElementsByClassName('seatmap-occupancy-detail')).scope();
				seatmapOccupancyScope.vm.previousSelectedSeatIds = [];
				getObjectsByType(canvas, 'seat').slice(0, seatNumber).forEach(function (seat) {
					seatmapOccupancyScope.vm.previousSelectedSeatIds.push(seat.id);
				});
				seatmapOccupancyScope.vm.selectSeat();
			}

			return utils;
		}]);
}());
