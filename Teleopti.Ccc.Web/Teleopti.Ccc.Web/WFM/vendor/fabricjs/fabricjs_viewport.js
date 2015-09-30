// Generated by CoffeeScript 1.7.1
(function () {
	var drawControls, old, _drawControl, _setCornerCoords,
	  __hasProp = {}.hasOwnProperty,
	  __extends = function (child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

	_drawControl = fabric.Object.prototype._drawControl;

	fabric.Object.prototype._drawControl = function (control, ctx, methodName, left, top) {
		var zoom, _ref;
		zoom = ((_ref = this.canvas.viewport) != null ? _ref.zoom : void 0) || 1;
		ctx.lineWidth = 1 / Math.max(this.scaleX, this.scaleY) / zoom;
		return _drawControl.apply(this, [control, ctx, methodName, left, top]);
	};

	drawControls = fabric.Object.prototype.drawControls;

	fabric.Object.prototype.drawControls = function (ctx) {
		var ret, zoom, _ref, _ref1;
		zoom = ((_ref = this.canvas) != null ? (_ref1 = _ref.viewport) != null ? _ref1.zoom : void 0 : void 0) || 1;
		this.cornerSize = this.cornerSize / zoom;
		ret = drawControls.apply(this, [ctx]);
		this.cornerSize = this.cornerSize * zoom;
		return ret;
	};

	_setCornerCoords = fabric.Object.prototype._setCornerCoords;

	fabric.Object.prototype._setCornerCoords = function () {
		var ret, zoom, _ref, _ref1;
		zoom = ((_ref = this.canvas) != null ? (_ref1 = _ref.viewport) != null ? _ref1.zoom : void 0 : void 0) || 1;
		this.cornerSize = this.cornerSize / zoom;
		ret = _setCornerCoords.apply(this);
		this.cornerSize = this.cornerSize * zoom;
		return ret;
	};

	//fabric.Object.prototype.drawBorders = function (ctx) {
	//	var h, padding, padding2, rotateHeight, scaleX, scaleY, strokeWidth, w, zoom, _ref, _ref1;
	//	if (!this.hasBorders) {
	//		return this;
	//	}
	//	zoom = ((_ref = this.canvas) != null ? (_ref1 = _ref.viewport) != null ? _ref1.zoom : void 0 : void 0) || 1;
	//	padding = this.padding;
	//	padding2 = padding * 2;
	//	strokeWidth = ~~(this.strokeWidth / 2) * 2;
	//	ctx.save();
	//	ctx.globalAlpha = this.isMoving ? this.borderOpacityWhenMoving : 1;
	//	ctx.strokeStyle = this.borderColor;
	//	scaleX = 1 / this._constrainScale(this.scaleX);
	//	scaleY = 1 / this._constrainScale(this.scaleY);
	//	ctx.lineWidth = 1 / this.borderScaleFactor / zoom;
	//	ctx.scale(scaleX, scaleY);
	//	w = this.getWidth();
	//	h = this.getHeight();
	//	ctx.strokeRect((-(w / 2) - padding - strokeWidth / 2 * this.scaleX) - 0.5 / zoom, (-(h / 2) - padding - strokeWidth / 2 * this.scaleY) - 0.5 / zoom, (w + padding2 + strokeWidth * this.scaleX) + 1 / zoom, (h + padding2 + strokeWidth * this.scaleY) + 1 / zoom);
	//	if (this.hasRotatingPoint && this.isControlVisible('mtr') && !this.get('lockRotation') && this.hasControls) {
	//		rotateHeight = (this.flipY ? h + ((strokeWidth * this.scaleY) + (padding * 2)) * zoom : -h - ((strokeWidth * this.scaleY) + (padding * 2)) * zoom) / 2;
	//		ctx.beginPath();
	//		ctx.moveTo(0, rotateHeight);
	//		ctx.lineTo(0, rotateHeight + (this.flipY ? this.rotatingPointOffset : -this.rotatingPointOffset));
	//		ctx.closePath();
	//		ctx.stroke();
	//	}
	//	ctx.restore();
	//	return this;
	//};

	fabric.Viewport = (function () {
		function Viewport(canvas) {
			this.i = 0;
			this.position = new fabric.Point(0, 0);
			this.zoom = 1;
			this.canvas = canvas;
			this._resetGrab();
			true;
		}

		Viewport.prototype.grabStart = function (e) {
			return this.grabStartPointer = this.canvas.getPointer(e);
		};

		Viewport.prototype.grab = function (e) {
			return this.grabPointer = this.canvas.getPointer(e);
		};

		Viewport.prototype.grabEnd = function (e) {
			var diff;
			diff = new fabric.Point(this.canvas.getPointer(e).x - this.grabStartPointer.x, this.canvas.getPointer(e).y - this.grabStartPointer.y);
			this.position = this.position.add(diff);
			return this._resetGrab();
		};

		Viewport.prototype._resetGrab = function () {
			this.grabStartPointer = {
				x: 0,
				y: 0
			};
			return this.grabPointer = {
				x: 0,
				y: 0
			};
		};

		Viewport.prototype.setZoom = function (newZoom) {
			this._adjustPositionAfterZoom(newZoom);
			return this.zoom = newZoom;
		};

		Viewport.prototype.resetPosition = function () {
			this.position = new fabric.Point(0, 0);
		};

		Viewport.prototype._adjustPositionAfterZoom = function (newZoom) {
			var height, width;
			width = this.canvas.width;
			height = this.canvas.height;
			this.position.x += ((this.zoom * width) - (newZoom * width)) / 2;
			return this.position.y += ((this.zoom * height) - (newZoom * height)) / 2;
		};

		Viewport.prototype.translate = function () {
			return {
				x: (this.position.x + this.grabPointer.x - this.grabStartPointer.x) / this.zoom,
				y: (this.position.y + this.grabPointer.y - this.grabStartPointer.y) / this.zoom
			};
		};

		Viewport.prototype.transform = function (event) {

			var ex, touchProp;
			touchProp = event.type === 'touchend' ? 'changedTouches' : 'touches';
			if (event[touchProp] && event[touchProp][0]) {
				ex = {};
				ex[touchProp] = _.map(event[touchProp], (function (_this) {
					return function (t) {
						return _this._transformEventParams(t);
					};
				})(this));
				return $.extend($.Event(event.type), ex);
			} else {
				return $.extend($.Event(event.type), this._transformEventParams(event));
			}
		};

		Viewport.prototype._transformEventParams = function (e) {
			
			var offsetLeft, offsetTop;
			offsetTop = this.canvas.wrapperEl.getBoundingClientRect().top;
			offsetLeft = this.canvas.wrapperEl.getBoundingClientRect().left;

			// removed - just update event with appropriate values.
			return {
				which: 1,
				shiftKey: e.shiftKey,  // added
				clientX: (e.clientX - offsetLeft) / this.zoom + offsetLeft - this.translate().x,
				clientY: (e.clientY - offsetTop) / this.zoom + offsetTop - this.translate().y,
				pageX: e.pageX - this.translate().x,
				pageY: e.pageY - this.translate().y,
				screenX: e.screenX - this.translate().x,
				screenY: e.screenY - this.translate().y
			};
	

		};

		return Viewport;

	})();

	old = fabric.PencilBrush.prototype._render;

	fabric.PencilBrush.prototype._render = function () {
		var ctx;
		if (this.canvas.viewport) {
			ctx = this.canvas.contextTop;
			ctx.save();
			ctx.scale(this.canvas.viewport.zoom, this.canvas.viewport.zoom);
			ctx.translate(this.canvas.viewport.translate().x, this.canvas.viewport.translate().y);
			old.apply(this);
			return ctx.restore();
		} else {
			return old.apply(this);
		}
	};

	fabric.CanvasWithViewport = (function (_super) {
		__extends(CanvasWithViewport, _super);

		function CanvasWithViewport() {
			this.viewport = new fabric.Viewport(this);
			this.isGrabMode = false;
			this._isCurrentlyGrabbing = false;
			CanvasWithViewport.__super__.constructor.apply(this, arguments);
		}

		CanvasWithViewport.prototype.setZoom = function (zoom) {
			this.viewport.setZoom(zoom);
			return this.renderAll();
		};

		CanvasWithViewport.prototype.resetPosition = function () {
			this.viewport.resetPosition();
			return this.renderAll();
		};

		CanvasWithViewport.prototype._onMouseMoveInGrabMode = function (e) {
			if (this._isCurrentlyGrabbing) {
				this.viewport.grab(e);
				this.renderAll();
				return true;
			}
		};

		CanvasWithViewport.prototype._onMouseDownInGrabMode = function (e) {
			this._isCurrentlyGrabbing = true;
			return this.viewport.grabStart(e);
		};

		CanvasWithViewport.prototype._onMouseUpInGrabMode = function (e) {
			if (this._isCurrentlyGrabbing) {
				this.viewport.grabEnd(e);
				this._isCurrentlyGrabbing = false;
				this.renderAll();
			}
			return true;
		};

		CanvasWithViewport.prototype._draw = function (ctx, object) {
			ctx.save();
			ctx.scale(this.viewport.zoom, this.viewport.zoom);
			ctx.translate(this.viewport.translate().x, this.viewport.translate().y);
			CanvasWithViewport.__super__._draw.apply(this, arguments);
			return ctx.restore();
		};

		CanvasWithViewport.prototype._drawSelection = function () {
			var ctx;
			ctx = this.contextTop;
			ctx.save();
			ctx.scale(this.viewport.zoom, this.viewport.zoom);
			ctx.translate(this.viewport.translate().x, this.viewport.translate().y);
			CanvasWithViewport.__super__._drawSelection.apply(this, arguments);
			return ctx.restore();
		};

		CanvasWithViewport.prototype.__onMouseMove = function (e) {
			if (this.isGrabMode) {
				this._onMouseMoveInGrabMode(e);
				return;
			}
			return CanvasWithViewport.__super__.__onMouseMove.call(this, this.viewport.transform(e));
		};

		CanvasWithViewport.prototype.__onMouseDown = function (e) {

			if (this.isGrabMode) {
				this._onMouseDownInGrabMode(e);
				return;
			}
			return CanvasWithViewport.__super__.__onMouseDown.call(this, this.viewport.transform(e));
		};

		CanvasWithViewport.prototype.__onMouseUp = function (e) {

			if (this.isGrabMode) {
				this._onMouseUpInGrabMode(e);
				return;
			}
			
			return CanvasWithViewport.__super__.__onMouseUp.call(this, this.viewport.transform(e));
		};

		return CanvasWithViewport;

	})(fabric.Canvas);

}).call(this);
