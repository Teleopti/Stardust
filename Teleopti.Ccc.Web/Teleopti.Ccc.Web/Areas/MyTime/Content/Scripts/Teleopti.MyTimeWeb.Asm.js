﻿/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Notifier.js"/>
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
	var texts;

	function asmViewModel(yesterday) {
		var self = this;
		self.intervalPointer = null;

		self.loadViewModel = function () {
			Teleopti.MyTimeWeb.Ajax.Ajax({
				url: 'Asm/Today',
				dataType: "json",
				type: 'GET',
				data: { asmZero: self.yesterday().toJSON() },
				success: function (data) {
					self.hours(data.Hours);
					self._createLayers(data.Layers);

					$('.asm-outer-canvas').show();

					self.intervalPointer = setInterval(function () {
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
		self.yesterday = ko.observable(yesterday);
		self.canvasPosition = ko.computed(function () {
			var msSinceStart = self.now() - self.yesterday().getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});
		self.now.subscribe(function (currentMs) {
			var yesterdayPlus2Days = new Date(self.yesterday().getTime()).addDays(2);
			if (currentMs > yesterdayPlus2Days.getTime()) {
				var todayMinus1 = new Date(currentMs).addDays(-1).clearTime();
				self.yesterday(todayMinus1);
			}
		});
		self.yesterday.subscribe(function () {
			if (self.intervalPointer != null) {
				clearInterval(self.intervalPointer);
			}
			self.loadViewModel();
		});
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

	function _showAsm() {
		_setFixedElementAttributes();

		var yesterDayFromNow = new Date(new Date().getTeleoptiTime()).addDays(-1).clearTime();
		vm = new asmViewModel(yesterDayFromNow);
		ko.applyBindings(vm);
		vm.loadViewModel();
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', (3 * 24 * pixelPerHours));
		$('.asm-timeline-line').css('width', (pixelPerHours - 1)); //"1" due to border size
	}

	function _listenForEvents(listeners) {
		$(listeners).each(function () {
			Teleopti.MyTimeWeb.Ajax.Ajax({
				url: 'MessageBroker/FetchUserData',
				dataType: "json",
				type: 'GET',
				success: function (data) {
					Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
						url: data.Url,
						callback: $(this),
						domainType: 'IScheduleChangedInDefaultScenario',
						businessUnitId: data.BusinessUnitId,
						datasource: data.DataSourceName,
						referenceId: data.AgentId
					});
				}
			});
		});
	}

	function _validSchedulePeriod(notification) {
		var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate).addDays(-1);
		var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate).addDays(1);
		var listeningStartDate = new Date(new Date().getTeleoptiTime()).addHours(-1);
		var listeningEndDate = new Date(listeningStartDate.getTime()).addDays(1);

		if (messageStartDate < listeningEndDate && messageEndDate > listeningStartDate) {
			return true;
		}
		return false;
	}

	return {
		ShowAsm: function () {
			_showAsm();
		},
		ListenForScheduleChanges: function (userTexts, eventListeners) {
			texts = userTexts;
			_listenForEvents(eventListeners);
		},
		NotifyWhenScheduleChangedListener: function (notification) {
			if (_validSchedulePeriod(notification)) {
				Teleopti.MyTimeWeb.Notifier.Notify({ text: texts.yourScheduleHasChanged });
			}
		},
		ReloadAsmViewModelListener: function (notification) {
			if (_validSchedulePeriod(notification)) {
				vm.loadViewModel();
			}
		}
	};
})(jQuery);