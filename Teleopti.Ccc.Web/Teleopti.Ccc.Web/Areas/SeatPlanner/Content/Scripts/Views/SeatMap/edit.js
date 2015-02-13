define([
		'knockout',
		'resources',
		'guidgenerator',
		'fabric',
		'fabricJS/seat',
		'Views/SeatMap/utils'

], function (
		ko,
		resources,
		guidgenerator,
		fabric,
		seat,
		utils
	) {

	
	return new function () {

		var self = this;
		this.canvas = null;
		this.seatPriority = 0;

		this.Setup = function (canvas, document) {

			self.canvas = canvas;

			document.getElementById('imgLoader').onchange = self.HandleImageLoad;
			document.getElementById('backgroundLoader').onchange = self.HandleBackgroundLoad;
			document.getElementById('dataLoader').onchange = self.Import;

			self.CreateListenersKeyboard();
		};

		this.hasActiveGroup = ko.computed(function () {
			if (self.canvas) {
				return self.canvas.getActiveGroup() != null;
			}
			return false;
		});

		this.CreateListenersKeyboard = function () {
			document.onkeydown = self.OnKeyDownHandler;
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
				utils.ScaleImage(self.canvas, image);
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
				for (var i in self.copiedGroup.objects) {
					var objToCopy = self.copiedGroup.objects[i];
					var left = self.copiedGroup.left;
					var top = self.copiedGroup.top;
					objToCopy.clone(function (obj) {
						obj.set("top", top + obj.top + 15);
						obj.set("left", left + obj.left + 15);
						self.UpdateSeatPriorityOnPaste(obj);

						self.canvas.add(obj);
					});
					self.canvas.setActiveGroup(self.copiedGroup);
				}
			} else if (self.copiedObject) {
				self.copiedObject.clone(function (obj) {
					obj.set("top", obj.top + 15);
					obj.set("left", obj.left + 15);
					self.UpdateSeatPriorityOnPaste(obj);
					self.canvas.add(obj);
					self.canvas.setActiveObject(obj);
				});
			}

			self.canvas.renderAll();
		};

		this.UpdateSeatPriorityOnPaste = function (obj) {
			if (obj.type == 'group') {
				for (var i = 0; i < obj._objects.length; i++) {
					var childObj = obj._objects[i];
					self.UpdateSeatPriorityOnPaste(childObj);
				}

			} else {
				if (obj.type == 'seat') {
					self.seatPriority++;
					obj.set('priority', self.seatPriority);
				}
			}
		};

		this.RemoveSelected = function () {
			var activeObject = self.canvas.getActiveObject(),
				activeGroup = self.canvas.getActiveGroup();

			if (activeGroup) {
				var objectsInGroup = activeGroup.getObjects();
				self.canvas.discardActiveGroup();
				objectsInGroup.forEach(function (object) {
					self.canvas.remove(object);
				});
			}
			else if (activeObject) {
				self.canvas.remove(activeObject);
			}
		};


		this.Clear = function () {
			self.canvas.setBackgroundImage(null);
			self.canvas.clear();
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
				var group = new fabric.Group(objectsInGroup, { left: 200, top: 200 });
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

		//Alignment, Rotation and Flip

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
				activeGroup.forEachObject(function(o) {
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
				var offset = 12;
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
				var offset = 10;
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
				name: 'foobar',
				priority: self.seatPriority,
				guid: guidgenerator.newGuid()
			};

			fabric.util.loadImage(imgName, function (img) {
				var newSeat = new fabric.Seat(img, seatObj);
				self.canvas.add(newSeat);
				newSeat.center();
				newSeat.setCoords();
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

		//Serialize

		this.Export = function () {
			var json = JSON.stringify(self.canvas);
			// hack to get basic file export working.
			var downloadLink = document.createElement("a");
			var blob = new Blob(["\ufeff", json]);
			var url = URL.createObjectURL(blob);
			downloadLink.href = url;
			downloadLink.download = "seatMap.map";
			document.body.appendChild(downloadLink);
			downloadLink.click();
			document.body.removeChild(downloadLink);
		}

		this.Import = function (e) {
			var reader = new FileReader();
			reader.onload = function (event) {
				var json = event.target.result;
				self.seatPriority = 0;
				self.canvas.loadFromJSON(json, function () {
					self.canvas.renderAll();
					var allSeats = self.GetObjectsByType('seat');
					for (var loadedSeat in allSeats) {
						if (allSeats[loadedSeat].priority > self.seatPriority) {
							self.seatPriority = allSeats[loadedSeat].priority;
						}
					}
				});
			}
			reader.readAsText(e.target.files[0]);

		}

		this.GetObjectsByType = function (type) {
			var canvasObjects = self.canvas.getObjects();
			var objectsArray = new Array();

			for (obj in canvasObjects) {
				if (canvasObjects[obj].get('type') == type) {
					objectsArray.push(canvasObjects[obj]);
				}

				if (canvasObjects[obj].get('type') == 'group') {
					var groupObjects = canvasObjects[obj].getObjects();
					for (var groupObj in groupObjects) {
						if (groupObjects[groupObj].get('type') == type) {
							objectsArray.push(groupObjects[groupObj]);
						}
					}
				}
			}
			return objectsArray;
		}


	};
});

