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
	'fabricJS/seat',
	'Views/SeatMap/edit',
	'Views/SeatMap/utils',
	'ajax'
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
	seat,
	seatMapEdit,
	utils,
	ajax
	) {

	return function () {
		var self = this;
		this.allowEdit = ko.observable(false);
		this.drawGrid = ko.observable(false);
		this.seatMapEditor = seatMapEdit;
		this.canvasUtils = utils;
		this.seatMapId = null;
		var options;
		this.Resources = resources;
		this.Loading = ko.observable(false);
		this.breadcrumb = ko.observableArray([]);
		this.SetViewOptions = function (opts) {
			options = opts;
		};

		//this.SetDefaultBackground = function () {
		//	fabric.Image.fromURL('Areas/SeatPlanner/Content/Images/background.svg', function (image) {
		//		self.canvasUtils.ScaleImage(self.canvas, image);
		//		self.canvas.setBackgroundImage(image);
		//		self.canvas.centerObject(image);
		//	});
		//};

		this.ToggleEditMode = function () {
			self.allowEdit(!self.allowEdit());
			self.canvasUtils.SetSelectionMode(self.canvas, self.allowEdit());
		};

		this.ResizeCanvas = function () {
			var container = self.canvas.wrapperEl.parentElement;
			self.canvas.setHeight($(container).height());
			self.canvas.setWidth(container.clientWidth);
			
			if (self.drawGrid()) {
				self.canvasUtils.DrawGrid(self.canvas);
			}
		};

		this.RefreshSeatMap = function() {
			self.LoadSeatMapFromId(self.seatMapId);
		};

		this.SetupCanvas = function () {

			self.canvas = new fabric.CanvasWithViewport("c");
			self.canvas.isGrabMode = false;
			self.seatMapEditor.Setup(self.canvas, $(document)[0], options, self.RefreshSeatMap);

			//self.canvas.renderOnAddRemove = false;  // performance toggle
			self.canvas.stateful = false; // perhaps add if need undo
			//self.SetDefaultBackground();

			$(document).ready(function () {
				$(window).resize(self.ResizeCanvas);

				$('#zoomSlider').bind('input', function () {
					self.Zoom(this.value);
				});

				$('#zoomSlider').change(function () {
					self.Zoom(this.value);
				});

				self.ResizeCanvas();
			});

			self.SetupHandleLocationClick();
			self.CreateListenersKeyboard();

		};

		this.SetupHandleLocationClick = function () {

			self.canvas.on('mouse:down', function (e) {
				if (!self.allowEdit()) {
					var location = self.canvasUtils.GetFirstObjectOfTypeFromCanvasObject(e.target, "location");
					if (location != null) {
						self.LoadSeatMapOnLocationClick(location);
					}
				};
			});
		};

		this.HandleBreadcrumbClick = function(data) {
			self.LoadSeatMapFromId(data.Id);
		};

		this.LoadSeatMapOnLocationClick = function (location) {
			self.LoadSeatMapFromId(location.seatMapId);
		};

		this.LoadParentSeatMap = function () {
			self.LoadSeatMapFromId(self.parentId);
		};

		this.LoadSeatMapFromId = function (id) {

			self.Loading(true);

			ajax.ajax({
				url: "SeatPlanner/SeatMap/Get?id=" + id,
				success: function (data) {
					self.LoadSeatMap(data);
				}
			});
		};

		this.CreateListenersKeyboard = function () {
			document.onkeydown = self.OnKeyDownHandler;
		};

		this.OnKeyDownHandler = function (event) {

			if (self.allowEdit()) {
				self.seatMapEditor.OnKeyDownHandler(event);
			}

		};

		this.LoadSeatMap = function (data) {
			if (data != null) {

				self.parentId = data.ParentId;
				self.seatMapId = data.Id;

				self.canvasUtils.LoadSeatMap(self.canvas, data, self.allowEdit(), self.ContinueLoadAfterCanvasLoaded);

			}
			else
			{
				self.Loading(false);
				self.ResetZoom();
			}
		};


		this.ContinueLoadAfterCanvasLoaded = function(data)
		{
			self.seatMapEditor.LoadExistingSeatMapData(data);
			self.breadcrumb(data.BreadcrumbInfo);

			self.Loading(false);
			self.ResetZoom();

		}
		
		//Zoom and Move

		this.ResetZoom = function() {
			self.canvas.setZoom(1);
		};

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

	};
});
