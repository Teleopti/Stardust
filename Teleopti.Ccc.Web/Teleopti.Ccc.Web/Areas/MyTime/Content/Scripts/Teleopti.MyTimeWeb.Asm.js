/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
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
					alert('nope');
				}
			});
		};

		self._createLayers = function (layers) {
			var newLayers = new Array();
			$.each(layers, function (key, layer) {
				newLayers.push(new layerViewModel(layer));
			});
			self.layers(newLayers);
		};
		
		self.hours = ko.observableArray();
		self.layers = ko.observableArray();
		self.now = ko.observable();
		self.canvasPosition = ko.computed(function () {
			var msSinceStart = self.now() - yesterday.getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});
		self.yesterday = yesterday;

		self.activityInfo = ko.computed(function () {
			var info = new Array();
			var timelinePosition = timeLineMarkerWidth - parseFloat(self.canvasPosition());
			$.each(self.layers(), function (key, layer) {
				var startPos = parseInt(layer.leftPx);
				var endPos = startPos + parseInt(layer.paddingLeft);
				if (endPos > timelinePosition) {
					var active = false;
					if (startPos <= timelinePosition) {
						active = true;
					}
					var startText = layer.startTimeText;
					if (startPos - timelinePosition >= 24 * pixelPerHours) {
						startText += '+1';
					}
					info.push({ 'payload': layer.payload, 'time': startText, 'active': active });
				}
			});
			return info;
		});
	}

	function layerViewModel(layer) {
		var self = this;

		self.startMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero;
		self.leftPx = (layer.StartMinutesSinceAsmZero * pixelPerHours / 60 + timeLineMarkerWidth) + 'px';
		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.paddingLeft = (layer.LengthInMinutes * pixelPerHours) / 60 + 'px';
		self.startTimeText = layer.StartTimeText;
		self.endTimeText = layer.EndTimeText;
		self.title = layer.startTimeText + '-' + layer.endTimeText + ' ' + layer.payload;
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
		}
	};
})(jQuery);