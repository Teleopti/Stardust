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
	var vm;

	function asmViewModel(yesterday) {
		var self = this;

		self.loadViewModel = function () {
			Teleopti.MyTimeWeb.Ajax.Ajax({
				url: 'Asm/Today',
				dataType: "json",
				type: 'GET',
				data: { asmZero: yesterday.toJSON() },
				success: function (data) {
					self.hours(data.Hours);
					self._createLayers(data.Layers);

					$('.asm-outer-canvas').show();

					setInterval(function () {
						self.now(new Date().getTeleoptiTime());
					}, 1000 * refreshSeconds);
				}
			});
		};

		self._createLayers = function (layers) {
			var newLayers = new Array();
			$.each(layers, function (key, layer) {
				newLayers.push(new layerViewModel(layer, self.canvasPosition));
			});
			self.layers(newLayers);
		};

		self.hours = ko.observableArray();
		self.layers = ko.observableArray();
		self.visibleLayers = ko.computed(function () {
			return $.grep(self.layers(), function (n, i) {
				return n.visible();
			});
		});
		self.now = ko.observable(new Date().getTeleoptiTime());
		self.canvasPosition = ko.computed(function () {
			var msSinceStart = self.now() - yesterday.getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});
		self.yesterday = yesterday;
	}

	function layerViewModel(layer, canvasPosition) {
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
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition());
			var startPos = parseFloat(self.leftPx);
			var endPos = startPos + parseFloat(self.paddingLeft);
			return endPos > timelinePosition;
		});
		self.active = ko.computed(function () {
			if (!self.visible)
				return false;
			var startPos = parseFloat(self.leftPx);
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition());
			var isActive = startPos <= timelinePosition;
			return isActive;
		});

		self.isNextday = ko.computed(function () {
			return layer.StartMinutesSinceAsmZero > 2 * 24 * 60;
		});
	}

	function _start() {
		_setFixedElementAttributes();

		var yesterdayTemp = new Date(new Date().getTeleoptiTime());
		yesterdayTemp.setDate(yesterdayTemp.getDate() - 1);
		var yesterday = new Date(yesterdayTemp.getFullYear(), yesterdayTemp.getMonth(), yesterdayTemp.getDate());

		vm = new asmViewModel(yesterday);
		ko.applyBindings(vm);
		vm.loadViewModel();
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', (3 * 24 * pixelPerHours));
		$('.asm-timeline-line').css('width', (pixelPerHours - 1)); //"1" due to border size
	}

	function _listenForEvents() {
		var onMessageBrokerEvent = function (notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			messageStartDate.setDate(messageStartDate.getDate() - 1);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);
			messageEndDate.setDate(messageEndDate.getDate() + 1);
			var visibleStartDate = vm.yesterday;
			var visibleEndDate = new Date(visibleStartDate.getTime());
			visibleEndDate.setDate(visibleEndDate.getDate() + 2);

			if (messageStartDate < visibleEndDate && messageEndDate > visibleStartDate) {
				vm.loadViewModel();
			}
		};

		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: '/MyTime/MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: onMessageBrokerEvent,
					domainType: 'IScheduleChangedInDefaultScenario',
					businessUnitId: data.BusinessUnitId,
					datasource: data.DataSourceName,
					referenceId: data.AgentId
				});
			}
		});
	}

	return {
		Init: function () {
			_start();
			_listenForEvents();
		}
	};
})(jQuery);