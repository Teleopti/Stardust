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
	'Views/SeatMap/utils'
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
	utils
	) {

	return function () {
		var self = this;
		this.allowEdit = ko.observable(true);
		this.seatMapEditor = seatMapEdit;
		var businessUnitId;
		this.Resources = resources;
		this.Loading = ko.observable(false);
		this.SetViewOptions = function (options) {
			businessUnitId = options.buid;
		};

		this.SetDefaultBackground = function () {
			fabric.Image.fromURL('Areas/SeatPlanner/Content/Images/background.svg', function (image) {
				utils.ScaleImage(self.canvas, image);
				self.canvas.setBackgroundImage(image);
				self.canvas.centerObject(image);
			});
		};

		this.ResizeCanvas = function () {
			var container = self.canvas.wrapperEl.parentElement;
			self.canvas.setHeight($(container).height());
			self.canvas.setWidth($(container).width());
		};

		this.SetupCanvas = function () {

			self.canvas = new fabric.CanvasWithViewport("c");
			self.canvas.isGrabMode = false;

			if (self.allowEdit()) {
				self.seatMapEditor.Setup(self.canvas, $(document)[0]);
			}

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

	};
});
