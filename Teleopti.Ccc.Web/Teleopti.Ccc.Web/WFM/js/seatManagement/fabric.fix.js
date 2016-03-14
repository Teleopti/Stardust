(function () {

	/*  override of fabric.util.getScrollLeftTop */
	fabric.util.getScrollLeftTop = function (element, upperCanvasEl) {

		var firstFixedAncestor,
			origElement,
			left = 0,
			top = 0,
			docElement = fabric.document.documentElement,
			body = fabric.document.body || {
				scrollLeft: 0, scrollTop: 0
			};

		origElement = element;

		while (element && element.parentNode && !firstFixedAncestor) {

			element = element.parentNode;

			if (element.nodeType === 1 &&
				fabric.util.getElementStyle(element, 'position') === 'fixed') {
				firstFixedAncestor = element;
			}

			if (element.nodeType === 1 &&
				origElement !== upperCanvasEl &&
				fabric.util.getElementStyle(element, 'position') === 'absolute') {
				left = 0;
				top = 0;
			}
			else if (element === fabric.document) {
				// overrride - was not calculating scroll position correctly!
				//left = body.scrollLeft || docElement.scrollLeft || 0;
				//top = body.scrollTop || docElement.scrollTop || 0;

				left = docElement.scrollLeft || 0;
				top = docElement.scrollTop || 0;
			}
			else {
				left += element.scrollLeft || 0;
				top += element.scrollTop || 0;
			}
		}

		return { left: left, top: top };
	}

	//override to handle pan on right click...
	fabric.CanvasWithViewport.prototype.__onMouseDown = function (e) {

		if (e.which === 3) {
			this.isGrabMode = true;
		};

		if (this.isGrabMode) {
			this._onMouseDownInGrabMode(e);
			return;
		}
		return fabric.CanvasWithViewport.__super__.__onMouseDown.call(this, this.viewport.transform(e));
	};

	fabric.CanvasWithViewport.prototype.__onMouseUp = function (e) {

		if (this.isGrabMode) {
			if (e.which === 3) {
				this.isGrabMode = false;
			};

			this._onMouseUpInGrabMode(e);

			return;
		}

		return fabric.CanvasWithViewport.__super__.__onMouseUp.call(this, this.viewport.transform(e));
	};


	var canvasPrototype = fabric.Canvas.prototype;

	canvasPrototype._onDoubleClick = function (e) {
		var self = this;
		var target = self.findTarget(self.viewport.transform(e));
		self.fire('mouse:dblclick', {
			target: target,
			e: e
		});

		if (target && !self.isDrawingMode) {

			// To unify the behavior, the object's double click event does not fire on drawing mode.
			target.fire('object:dblclick', {
				e: e
			});
		}
	};

	var addListener = fabric.util.addListener;
	var removeListener = fabric.util.removeListener;

	
	canvasPrototype._existingEventListenerCall = canvasPrototype._initEventListeners;
	canvasPrototype._existingRemoveEventListenerCall = canvasPrototype.removeListeners;

	canvasPrototype._initEventListeners = function () {
		var self = this;
		self._existingEventListenerCall();
		addListener(self.upperCanvasEl, 'dblclick', self._onDoubleClick.bind(self));
	},

	canvasPrototype.removeListeners = function () {
		var self = this;
		self._existingRemoveEventListenerCall();
		removeListener(self.upperCanvasEl, 'dblclick', self._onDoubleClick.bind(self));
	}


	var imagePrototype = fabric.Image.prototype;
	imagePrototype._render = function(ctx, noTransform) {
		var x, y, imageMargins = this._findMargins(), elementToDraw;

		x = (noTransform ? this.left : -this.width / 2);
		y = (noTransform ? this.top : -this.height / 2);

		if (this.meetOrSlice === 'slice') {
			ctx.beginPath();
			ctx.rect(x, y, this.width, this.height);
			ctx.clip();
		}

		if (this.isMoving === false && this.resizeFilters.length && this._needsResize()) {
			this._lastScaleX = this.scaleX;
			this._lastScaleY = this.scaleY;
			elementToDraw = this.applyFilters(null, this.resizeFilters, this._filteredEl || this._originalElement, true);
		} else {
			elementToDraw = this._element;
		}

		var canvas = this.canvas;

		// I Love IE....there can be a timing issue in IE between it loading the image and drawing it.
		// catch when this occurs, and retry the render with a timeout.
		try {

			elementToDraw && ctx.drawImage(elementToDraw,
				x + imageMargins.marginX,
				y + imageMargins.marginY,
				imageMargins.width,
				imageMargins.height
			);

			this._renderStroke(ctx);

		} catch (err) {
			
			setTimeout(function() {
				imagePrototype._render(ctx, noTransform);
				canvas.renderAll();
			}, 0);
		}

	};




}());