define([
	'fabric'
], function (
	fabric
) {
	fabric.Object.prototype.transparentCorners = false;
	fabric.Object.prototype.lockScalingFlip = true;
	fabric.Object.prototype.hasBorders = false;

	return new function () {

		var self = this;

		this.ScaleImage = function (canvas, image) {

			var ratio = canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});

		};

		this.DrawGrid = function (canvas) {
			var grid = 30;

			// create grid

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

		this.SetSelectionMode = function (canvas, allowSelection) {
			var canvasObjects = canvas.getObjects();
			for (var idx in canvasObjects) {
				canvasObjects[idx].selectable = allowSelection;
			}

		};

		this.GetObjectsByType = function (canvas, type) {
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

		this.GetObjectsOfTypeFromCanvasObject = function (obj, type) {

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

		this.GetObjectsOfTypeFromGroup = function(group, type) {
		
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

		this.GetFirstObjectOfTypeFromCanvasObject = function (obj, type) {

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

		this.ClearCanvas = function (canvas) {
			if (canvas != null) {
				canvas.setBackgroundImage(null);
				canvas.clear();
			}
		}

		this.LoadSeatMap = function (canvas, data, allowSelection, callback) {

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

	}
});