/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	var refreshSeconds = 1;
	var pixelPerHours = 50;
	var timeLineMarkerWidth = 100;

	function asmViewModel(yesterday) {
		var self = this;

		self.loadViewModel = function () {
			Teleopti.MyTimeWeb.Ajax.Ajax({
				url: '/MyTime/Asm/Today',
				dataType: "json",
				type: 'GET',
				data: { asmZero: yesterday.toJSON() },
				success: function (data) {
					self.now(new Date().getTeleoptiTime());
					self.hours(data.Hours);
					self._createLayers(data.Layers);

					$('.asm-outer-canvas').show();

					setInterval(function () {
						self.now(new Date().getTeleoptiTime());
					}, 1000 * refreshSeconds);
				},
				error: function () {
					alert('nope'); //todo: wad ska hända här? (om ingen kontakt med servern)
				}
			});
		};

		self._createLayers = function (layers) {
			var newLayers = new Array();
			$.each(layers, function (key, layer) {
				newLayers.push(new layerViewModel(layer, self));
			});
			self.layers(newLayers);
		};

		self.hours = ko.observableArray();
		self.layers = ko.observableArray();
		self.activeLayers = ko.computed(function () {
			return $.grep(self.layers(), function (n, i) {
				return n.visible();
			});
		});
		self.now = ko.observable();
		self.canvasPosition = ko.computed(function () {
			var msSinceStart = self.now() - yesterday.getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});
		self.yesterday = yesterday;
	}

	function layerViewModel(layer, canvas) {
		var self = this;

		self.leftPx = (layer.StartMinutesSinceAsmZero * pixelPerHours / 60 + timeLineMarkerWidth) + 'px';
		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.paddingLeft = (layer.LengthInMinutes * pixelPerHours) / 60 + 'px';
		self.startTimeText = layer.StartTimeText;
		self.endTimeText = layer.EndTimeText;
		self.title = layer.StartTimeText + '-' + layer.EndTimeText + ' ' + layer.Payload;
		self.visible = ko.computed(function () {
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvas.canvasPosition());
			var startPos = parseFloat(self.leftPx);
			var endPos = startPos + parseFloat(self.paddingLeft);
			return endPos > timelinePosition;
		});
		self.active = ko.computed(function () {
			if (!self.visible)
				return false;
			var startPos = parseFloat(self.leftPx);
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvas.canvasPosition());
			return startPos <= timelinePosition;
		});
		self.startText = ko.computed(function () {
			var startPos = parseFloat(self.leftPx);
			var out = self.startTimeText;
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvas.canvasPosition());
			if (startPos - timelinePosition >= 24 * pixelPerHours) {
				out += '+1';
			}
			return out;
		});
	}

	function _start() {
		_setFixedElementAttributes();

		var yesterdayTemp = new Date(new Date().getTeleoptiTime());
		yesterdayTemp.setDate(yesterdayTemp.getDate() - 1);
		var yesterday = new Date(yesterdayTemp.getFullYear(), yesterdayTemp.getMonth(), yesterdayTemp.getDate());

		var vm = new asmViewModel(yesterday);
		vm.loadViewModel();
		ko.applyBindings(vm);
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', (3 * 24 * pixelPerHours));
		$('.asm-timeline-line').css('width', (pixelPerHours - 1)); //"1" due to border size
	}

	return {
		Init: function () {
			_start();

			var onevent = function (notification) {
				console.log(JSON.stringify(notification));
			};

			Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
				url: 'http://localhost:54903/signalr',
				callback: onevent
			}, {
				domainType: 'IPersistableScheduleData',
				businessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
				datasource: 'Teleopti CCC - åhå jaja'
			});
		}
	};
})(jQuery);