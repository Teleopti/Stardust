define([
	'knockout',
	'jquery',
	'navigation',
	'lazy',
	'resources',
	'moment',
	'momentTimezoneData',
	'fabric',
	'fabricViewport',
	'fabricJS/seat'
], function (
	ko,
	$,
	navigation,
	lazy,
	resources,
	moment,
	momentTimezoneData,
	fabric,
	fabricViewport,
	seat
	) {

	return function () {
		var self = this;
		var businessUnitId;
		this.seatPriority = 0;
		this.Resources = resources;
		this.Loading = ko.observable(false);
		this.SetViewOptions = function (options) {
			businessUnitId = options.buid;
		};

		this.SetDefaultBackground = function () {
			fabric.Image.fromURL('Areas/SeatPlanner/Content/Images/background.svg', function (image) {
				self.ScaleImage(image);
				self.canvas.setBackgroundImage(image);
				self.canvas.centerObject(image);
			});
		};

		this.ScaleImage = function (image) {

			var ratio = self.canvas.height / image.height;
			image.set({
				scaleY: ratio,
				scaleX: ratio
			});
		};

		this.RespondCanvas = function () {
			var container = self.canvas.wrapperEl.parentElement;
			self.canvas.setHeight($(container).height());
			self.canvas.setWidth($(container).width());

		};

		this.SetupCanvas = function () {
			fabric.Object.prototype.transparentCorners = false;
			fabric.Object.prototype.lockScalingFlip = true;
			fabric.Object.prototype.hasBorders = false;

			self.canvas = new fabric.CanvasWithViewport("c");
			self.canvas.isGrabMode = false;
			//self.canvas.renderOnAddRemove = false;  // performance toggle
			self.canvas.stateful = false; // perhaps add if need undo

			//self.SetDefaultBackground();
			self.CreateListenersKeyboard();

			$(document).ready(function () {
				$(window).resize(self.RespondCanvas);

				$('#zoomSlider').bind('input', function () {
					self.Zoom(this.value);
				});

				$('#zoomSlider').change(function () {
					self.Zoom(this.value);
				});

				self.RespondCanvas();
			});

			$(document)[0].getElementById('imgLoader').onchange = self.HandleImageLoad;
			$(document)[0].getElementById('backgroundLoader').onchange = self.HandleBackgroundLoad;
			$(document)[0].getElementById('dataLoader').onchange = self.Import;

		};

		this.HandleImageLoad = function (e) {
			var reader = new FileReader();
			reader.onload = function (event) {
				var imgObj = new Image();
				imgObj.src = event.target.result;
				imgObj.onload = function () {
					var image = new fabric.Image(imgObj);
					image.set({
						left: 250,
						top: 250
					})
						.setCoords();

					self.canvas.add(image);
					self.canvas.renderAll();
				};
			};

			reader.readAsDataURL(e.target.files[0]);
		}


		this.HandleBackgroundLoad = function (e) {
			var reader = new FileReader();
			reader.onload = function (event) {
				var imgObj = new Image();
				imgObj.src = event.target.result;
				imgObj.onload = function () {
					var image = new fabric.Image(imgObj);
					self.ScaleImage(image);
					self.canvas.setBackgroundImage(image);
					self.canvas.centerObject(image);
				}
			}
			reader.readAsDataURL(e.target.files[0]);
		}

		this.CreateListenersKeyboard = function () {
			document.onkeydown = self.OnKeyDownHandler;
		};

		this.OnKeyDownHandler = function (event) {
			//event.preventDefault();
			var key;
			if (window.event) {
				key = window.event.keyCode;
			} else {
				key = event.keyCode;
			}
			switch (key) {
				case 67: // Ctrl+C
					if (self.AbleToShortcut()) {
						if (event.ctrlKey) {
							event.preventDefault();
							self.Copy();
						}
					}
					break;
					// Paste (Ctrl+V)
				case 86: // Ctrl+V
					if (self.AbleToShortcut()) {
						if (event.ctrlKey) {
							event.preventDefault();
							self.Paste();
						}
					}
					break;

				case 46: // delete
					self.RemoveSelected();
					break;


				default:
					// TODO
					break;
			}
		};

		this.AbleToShortcut = function () {
			/*
			TODO check all cases for this

			if($("textarea").is(":focus")){
				return false;
			}
			if($(":text").is(":focus")){
				return false;
			}
			*/
			return true;
		};

		this.Copy = function () {
			if (self.canvas.getActiveGroup()) {
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


		//Zoom and Move

		this.Zoom = function (value) {
			self.canvas.setZoom(value);
		};

		this.ZoomIn = function () {
			self.canvas.setZoom(self.canvas.viewport.zoom * 1.1);

		};

		this.ZoomOut = function () {
			self.canvas.setZoom(self.canvas.viewport.zoom * 0.9);
		};


		this.ToggleMoveMode = function () {
			self.canvas.isGrabMode = !self.canvas.isGrabMode;
			self.canvas.isGrabMode ? $("canvas").css("cursor", "move") : $("canvas").css("cursor", "pointer");
		};


		// Layers

		this.SendBackwards = function () {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject) {
				self.canvas.sendBackwards(activeObject);
			}
		};

		this.SendToBack = function () {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject) {
				self.canvas.sendToBack(activeObject);
			}
		};

		this.BringForwards = function () {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject) {
				self.canvas.bringForward(activeObject);
			}
		};

		this.BringToFront = function () {
			var activeObject = self.canvas.getActiveObject();
			if (activeObject) {
				self.canvas.bringToFront(activeObject);
			}
		};


		//Grouping

		this.GroupActiveObjects = function () {
			var activeGroup = self.canvas.getActiveGroup();
			var objectsInGroup;
			if (activeGroup) {
				objectsInGroup = activeGroup.getObjects();
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
			// remove the original group and add all items back to the canvas
			self.canvas.remove(group);
			for (var i = 0; i < items.length; i++) {
				self.canvas.add(items[i]);
			}
		};

		//Alignment, Rotation and Flip

		this.AlignLeft = function () {
			if (self.canvas.getActiveGroup()) {
				var left = 0;
				self.canvas.getActiveGroup().forEachObject(function (o) {
					if (o.left < left) {
						left = o.left;
					}
				});
				if (self.canvas.getActiveGroup().forEachObject(function (o) { o.set("left", left); }));

				self.canvas.renderAll();
			}
		};

		this.AlignRight = function () {
			if (self.canvas.getActiveGroup()) {
				var left = 0;
				self.canvas.getActiveGroup().forEachObject(function (o) {
					if (o.left > left) {
						left = o.left;
					}
				});
				if (self.canvas.getActiveGroup().forEachObject(function (o) { o.set("left", left); }));

				self.canvas.renderAll();
			}
		};

		this.AlignTop = function () {
			if (self.canvas.getActiveGroup()) {
				var top = 0;
				self.canvas.getActiveGroup().forEachObject(function (o) {
					if (o.top < top) {
						top = o.top;
					}
				});
				self.canvas.getActiveGroup().forEachObject(function (o) { o.set("top", top); });

				self.canvas.renderAll();
			}
		};

		this.AlignBottom = function () {
			if (self.canvas.getActiveGroup()) {
				var top = 0;
				self.canvas.getActiveGroup().forEachObject(function (o) {
					if (o.top > top) {
						top = o.top;
					}
				});
				if (self.canvas.getActiveGroup().forEachObject(function (o) { o.set("top", top); }));

				self.canvas.renderAll();
			}
		};

		this.SpaceEvenlyHorizontal = function () {
			if (self.canvas.getActiveGroup()) {
				var left = 0;
				var offset = 12;
				var group = self.canvas.getActiveGroup();
				var maxRange = group.size() - 1;
				for (var i = maxRange; i > -1; i--) {
					var o = group.objects[i];
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

			angle = angle > 360 ? 90 : angle < 0 ? 270 : angle;

			obj.setAngle(angle).setCoords();

			if (resetOrigin) {
				obj.setCenterToOrigin && obj.setCenterToOrigin();
			}

			self.canvas.renderAll();

		};

		this.FlipX = function () {
			if (self.canvas.getActiveGroup()) {
				self.canvas.getActiveGroup().toggle('flipX');
			} else {
				self.canvas.getActiveObject().toggle('flipX');
			}
			self.canvas.renderAll();
		}

		this.FlipY = function () {
			if (self.canvas.getActiveGroup()) {
				self.canvas.getActiveGroup().toggle('flipY');
			} else {
				self.canvas.getActiveObject().toggle('flipY');
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
				guid: null
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
