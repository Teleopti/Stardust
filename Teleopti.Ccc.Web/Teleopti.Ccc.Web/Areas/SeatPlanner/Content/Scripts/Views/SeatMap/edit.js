define([
		'knockout',
		'resources',
		'guidgenerator',
		'fabric',
		'fabricJS/seat',
		'fabricJS/location',
		'Views/SeatMap/utils',
		'Views/SeatMap/save'

], function (
		ko,
		resources,
		guidgenerator,
		fabric,
		seat,
		location,
		utils,
		save
	) {


	return new function () {

		var self = this;
		this.canvas = null;
		this.id = null;
		this.locationId = null;
		this.seatPriority = 0;
		this.locationName = ko.observable();
		this.businessUnitId = null;
		this.canvasUtils = utils;
		this.newLocations = [];
		this.newSeats = [];
		this.refreshPageFunction = null;

		this.Setup = function (canvas, document, options, refreshPageFunction) {
			self.canvas = canvas;
			self.businessUnitId = options.buid;
			self.refreshPageFunction = refreshPageFunction;

			document.getElementById('imgLoader').onchange = self.HandleImageLoad;
			document.getElementById('backgroundLoader').onchange = self.HandleBackgroundLoad;
			//document.getElementById('dataLoader').onchange = self.Import;

		};

		this.hasActiveGroup = function () {
			if (self.canvas) {
				return self.canvas.getActiveGroup() != null;
			}
			return false;
		};

		this.OnKeyDownHandler = function (event) {
			//event.preventDefault();
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 67: // Ctrl+C
					if (event.ctrlKey) {
						event.preventDefault();
						self.Copy();
					}
					break;
				case 86: // Ctrl+V
					if (event.ctrlKey) {
						event.preventDefault();
						self.Paste();
					}
					break;
				case 46: // delete
					self.RemoveSelected();
					break;
				default:
					break;
			}
		};

		this.HandleImageLoad = function (e) {
			var onLoadOfImage = function (imgObj) {
				var image = new fabric.Image(imgObj);
				image.set({
					left: 250,
					top: 250
				})
				.setCoords();

				self.canvas.add(image);
				self.canvas.renderAll();
			}

			self.HandleDataURLLoad(e, onLoadOfImage);
		};

		this.HandleBackgroundLoad = function (e) {
			var onLoadOfBackgroundImage = function (imgObj) {
				var image = new fabric.Image(imgObj);
				self.canvasUtils.ScaleImage(self.canvas, image);
				self.canvas.setBackgroundImage(image);
				self.canvas.centerObject(image);
			}

			self.HandleDataURLLoad(e, onLoadOfBackgroundImage);
		};

		this.HandleDataURLLoad = function (e, callback) {
			var reader = new FileReader();
			reader.onload = function (event) {
				var imgObj = new Image();
				imgObj.src = event.target.result;
				imgObj.onload = function () {
					callback(imgObj);
				}
			}

			reader.readAsDataURL(e.target.files[0]);
		}

		this.Copy = function () {
			if (self.hasActiveGroup()) {
				self.copiedGroup = self.canvas.getActiveGroup();
			} else {
				self.copiedObject = self.canvas.getActiveObject();
				self.copiedGroup = null;
			}
		};

		this.Paste = function () {
			if (self.copiedGroup) {
				self.CloneGroup(self.copiedGroup);
			} else if (self.copiedObject) {

				self.CloneObject(self.copiedObject);
				
			}
		};

		this.CloneObject = function (objectToClone) {

			objectToClone.clone(function (obj) {
				self.UpdateSeatDataOnPaste(obj);
				obj.set("top", obj.top + 35);
				obj.set("left", obj.left + 35);
				
				self.canvas.add(obj);
				self.canvas.setActiveObject(obj);
			});
		};

		this.CloneGroup = function(cloneGroup) {

			self.canvas.deactivateAll();

			var childObjects = [];
			var count = cloneGroup._objects.length;
			var cloneCount = 0;

			for (i in cloneGroup._objects) {
				cloneGroup._objects[i].clone(function (obj) {
					self.UpdateSeatDataOnPaste(obj);
					obj.set("top", obj.top + 35);
					obj.set("left", obj.left + 35);
					self.canvas.add(obj);
					childObjects.push(obj);
					cloneCount++;
					if (cloneCount == count) {
						self.AfterCloneGroup(childObjects);
					}
				});
			}
		}

		this.AfterCloneGroup = function(childObjects) {

			var group = new fabric.Group(childObjects.reverse(), {
				canvas: self.canvas
			});
			group.addWithUpdate();
			self.canvas.setActiveGroup(group, childObjects);
			group.saveCoords();
			self.canvas.renderAll();
		}



		this.UpdateSeatDataOnPaste = function (obj) {
			if (obj.type == 'group') {
				for (var i = 0; i < obj._objects.length; i++) {
					var childObj = obj._objects[i];
					self.UpdateSeatDataOnPaste(childObj);
				}

			} else {
				if (obj.type == 'seat') {
					self.seatPriority++;
					obj.set('priority', self.seatPriority);
					obj.set('id', guidgenerator.newGuid());
					self.newSeats.push(obj);
				}
			}
		};

		this.RemoveSelected = function () {
			var activeObject = self.canvas.getActiveObject(),
				activeGroup = self.canvas.getActiveGroup();

			if (activeGroup) {
				self.canvas.discardActiveGroup();
				self.RemoveObjectAndTidyReferences(activeGroup);
			}
			else if (activeObject) {
				self.canvas.remove(activeObject);
				self.RemoveObjectAndTidyReferences(activeObject);
			}
		};

		this.RemoveObjectAndTidyReferences = function (obj) {

			if (obj.get('type') == 'seat') {
				self.newSeats.splice($.inArray(obj, self.newSeats), 1);
			}

			if (obj.get('type') == 'location') {
				self.newLocations.splice($.inArray(obj, self.newLocations), 1);
			}

			if (obj.get('type') == 'group') {
				var objectsInGroup = obj.getObjects();
				objectsInGroup.forEach(function (child) {
					self.RemoveObjectAndTidyReferences(child);
					self.canvas.remove(child);
				});
			}
		};

		this.Clear = function () {
			self.canvasUtils.ClearCanvas(self.canvas);
		};

		// Layers

		this.SendBackwards = function () {
			self.ActionOnActiveObject(function (obj) { self.canvas.sendBackwards(obj); });
		};

		this.SendToBack = function () {
			self.ActionOnActiveObject(function (obj) { self.canvas.sendToBack(obj); });
		};

		this.BringForwards = function () {
			self.ActionOnActiveObject(function (obj) { self.canvas.bringForward(obj); });
		};

		this.BringToFront = function () {
			self.ActionOnActiveObject(function (obj) { self.canvas.bringToFront(obj); });
		};

		this.ActionOnActiveObject = function (func) {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject) {
				func(activeObject);
			}
		};

		//Grouping

		this.GroupActiveObjects = function () {
			var activeGroup = self.canvas.getActiveGroup();
			if (activeGroup) {
				var objectsInGroup = activeGroup.getObjects();
				self.canvas.discardActiveGroup();
				objectsInGroup.forEach(function (object) {
					self.canvas.remove(object);

				});
				var group = new fabric.Group(objectsInGroup, { left: activeGroup.left + 15, top: activeGroup.top + 15 });
				self.canvas.setActiveObject(group);
				self.canvas.add(group);
			}
		};

		this.UngroupActiveObjects = function () {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject && activeObject._objects) {
				self.UngroupObjects(activeObject);
			}
		};

		this.UngroupObjects = function (group) {
			var items = group._objects;
			// translate the group-relative coordinates to canvas relative ones
			group._restoreObjectsState();
			self.canvas.remove(group);
			for (var i = 0; i < items.length; i++) {
				self.canvas.add(items[i]);
			}
		};

		//Alignment, Spacing Rotation and Flip

		this.AlignLeft = function () {
			self.AlignHorizontal(true);
		};

		this.AlignRight = function () {
			self.AlignHorizontal(false);
		};

		this.AlignHorizontal = function (alignLeft) {
			var activeGroup = self.canvas.getActiveGroup();
			if (activeGroup) {
				var left = 0;
				activeGroup.forEachObject(function (o) {
					if (alignLeft) {
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

				if (activeGroup.forEachObject(function (o) { o.set("left", left); }));
				self.canvas.renderAll();
			}
		}

		this.AlignTop = function () {
			self.AlignVertical(true);
		};

		this.AlignBottom = function () {
			self.AlignVertical(false);
		};

		this.AlignVertical = function (alignTop) {
			var activeGroup = self.canvas.getActiveGroup();
			if (activeGroup) {
				var top = 0;
				activeGroup.forEachObject(function (o) {
					if (alignTop) {
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
					o.set("top", top);
				});

				self.canvas.renderAll();
			}
		};

		this.SpaceActiveGroupEvenlyHorizontal = function () {
			if (self.canvas.getActiveGroup()) {
				self.SpaceGroupEvenlyHorizontal(self.canvas.getActiveGroup().objects);
			}
		}

		this.SpaceGroupEvenlyHorizontal = function (objects) {
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
				self.canvas.renderAll();
			}
		};

		this.SpaceActiveGroupEvenlyVertical = function () {
			if (self.canvas.getActiveGroup()) {
				self.SpaceGroupEvenlyVertical(self.canvas.getActiveGroup().objects);
			}
		};

		this.SpaceGroupEvenlyVertical = function (objects) {
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
				self.canvas.renderAll();
			}
		};

		this.Rotate45 = function () {
			if (self.canvas.getActiveGroup()) {
				self.canvas.getActiveGroup().forEachObject(function (o) { self.RotateObject(o, 45); });
			} else {
				self.RotateObject(self.canvas.getActiveObject(), 45);
			}
		};

		this.RotateObject = function (obj, angleOffset) {
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

			self.canvas.renderAll();

		};

		this.FlipX = function () {
			self.Flip('flipX');
		}

		this.FlipY = function () {
			self.Flip('flipY');
		}

		this.Flip = function (flipToggleName) {
			if (self.canvas.getActiveGroup()) {
				self.canvas.getActiveGroup().toggle(flipToggleName);
			} else {
				self.canvas.getActiveObject().toggle(flipToggleName);
			}
			self.canvas.renderAll();
		}

		// Add Objects

		this.AddImage = function (imageName) {
			fabric.Image.fromURL('Areas/SeatPlanner/Content/Images/' + imageName, function (image) {
				image.set({
					left: 400,
					top: 400
				});
				self.canvas.add(image);

			});
		};

		this.AddSeat = function (withDesk) {

			self.seatPriority++;

			var imgName = 'Areas/SeatPlanner/Content/Images/';
			if (withDesk) {
				imgName += 'seatAndDesk.svg';
			} else {
				imgName += 'seat.svg';
			}

			var seatObj = {
				name: 'Unnamed seat',
				priority: self.seatPriority,
				id: guidgenerator.newGuid()
			};

			fabric.util.loadImage(imgName, function (img) {
				var newSeat = new fabric.Seat(img, seatObj);
				self.canvas.add(newSeat);
				newSeat.center();
				newSeat.setCoords();
				self.newSeats.push(newSeat);
			});
		};

		this.AddText = function (text) {
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

			self.canvas.add(textSample);
		};


		this.AddLocation = function () {

			$('#addLocationModal').on('shown.bs.modal', function () {
				$('#location').focus();
			});
			$('#addLocationModal').modal();

		};

		this.OnAddLocation = function () {
			self.AddLocationShape();
			$('#addLocationModal').modal('hide');
		};

		this.AddLocationShape = function () {

			var locationObj = {
				name: self.locationName(),
				id: guidgenerator.newGuid(),
				seatMapId: guidgenerator.newGuid(),
				isNew: true,
				height: 200,
				width: 300,
				fill: 'rgba(59, 111, 170, 0.2)'
			};

			var newLocation = new fabric.Location(locationObj);
			self.newLocations.push(newLocation);
			self.canvas.add(newLocation);
			newLocation.center();
			newLocation.setCoords();
		};

		//Serialize

		//this.Export = function() {
		//	var json = JSON.stringify(self.canvas);
		//	// hack to get basic file export working.
		//	var downloadLink = document.createElement("a");
		//	var blob = new Blob(["\ufeff", json]);
		//	var url = URL.createObjectURL(blob);
		//	downloadLink.href = url;
		//	downloadLink.download = "seatMap.map";
		//	document.body.appendChild(downloadLink);
		//	downloadLink.click();
		//	document.body.removeChild(downloadLink);
		//};

		//this.Import = function(e) {
		//	var reader = new FileReader();
		//	reader.onload = function(event) {
		//		var json = event.target.result;
		//		self.seatPriority = self.canvasUtils.LoadSeatMap(self.canvas, json);
		//	}
		//	reader.readAsText(e.target.files[0]);
		//};

		this.LoadExistingSeatMapData = function (data) {
			self.id = data.Id;
			self.seatPriority = data.seatPriority;
			self.locationId = data.Location;

			self.newSeats = [];
			self.newLocations = [];
		};

		this.Save = function () {
			var saveMgr = new save();

			var childLocations = [];
			var locations = self.canvasUtils.GetObjectsByType(self.canvas, 'location');
			for (var i in locations) {

				childLocations.push(
				{
					Id: locations[i].id,
					Name: locations[i].name,
					SeatMapId: locations[i].seatMapId,
					IsNew: (self.newLocations.indexOf(locations[i]) > -1)
				});
			}

			var seats = [];
			var seatObjects = self.canvasUtils.GetObjectsByType(self.canvas, 'seat');
			for (i in seatObjects) {
				seat = seatObjects[i];
				seats.push(
				{
					Id: seat.id,
					Name: seat.name,
					Priority: seat.priority,
					IsNew: (self.newSeats.indexOf(seat) > -1)
				});
			}

			var data = {
				SeatMapData: JSON.stringify(self.canvas),
				Id: self.id,
				Location: self.locationId,
				ChildLocations: childLocations,
				Seats: seats,
				BusinessUnitId: self.businessUnitId
			}

			saveMgr.SetData(data);
			saveMgr.Apply(self.OnSaveSuccess);
		};

		this.OnSaveSuccess = function () {
			self.refreshPageFunction.call();
		};

	};
});

