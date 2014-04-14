/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Notifier.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="../../../../Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/noty/jquery.noty.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AlertActivity = (function () {
	var timeLineMarkerWidth = 40;
	var pixelPerHours = 40;
	var alertvm;
	var notifyOptions;
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	function activityLayer(layer, canvasPosition) {
		var self = this;

		self.leftPx = (layer.StartMinutesSinceAsmZero * pixelPerHours / 60 + timeLineMarkerWidth) + 'px';
		self.paddingLeft = (layer.LengthInMinutes * pixelPerHours) / 60 + 'px';
		self.payload = layer.Payload;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.startTimeText = layer.StartTimeText;
		self.startMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero;

		self.visible = function () {
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition);
			var startPos = parseFloat(self.leftPx);
			var endPos = startPos + parseFloat(self.paddingLeft);
			return endPos > timelinePosition;
		};
		self.active = function () {
			if (!self.visible)
				return false;
			var startPos = parseFloat(self.leftPx);
			var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition);
			var isActive = startPos <= timelinePosition;
			return isActive;
		};
	}

	function notificationActivities(yesterday) {
		var self = this;
		self.now = new Date().getTeleoptiTime();
		self.yesterday = yesterday;
		self.alertTimeSetting = 60; //default setting 60secs
		self.canvasPosition = function () {
			var msSinceStart = self.now - self.yesterday.getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		};
		self.layers = new Array();
		self.layersRemovedPassed = function () {
			return $.grep(self.layers, function (n, i) {
				return n.visible();
			});
		};

		self.getTimeInterval = function (index) {
			var interval = 0;
			var length = self.layersRemovedPassed().length;
			if (index >= 0 && index < length) {
				var secondsSinceStart = (self.now - self.yesterday.getTime()) / 1000;
				var timeDiffrenceBetweenAsmZeroAndStart = self.layersRemovedPassed()[index].startMinutesSinceAsmZero * 60 - secondsSinceStart;

				if (timeDiffrenceBetweenAsmZeroAndStart < 0) {
					interval = -1;
				} else if (timeDiffrenceBetweenAsmZeroAndStart > self.alertTimeSetting) {
					interval = timeDiffrenceBetweenAsmZeroAndStart - self.alertTimeSetting;
				} else {
					interval = self.alertTimeSetting - timeDiffrenceBetweenAsmZeroAndStart;
				}
			}
			return interval;
		};

		self.alertMessage = function (index) {
			// TODO: replace the text with localed text
			return self.layersRemovedPassed()[index].payload + " at " + self.layersRemovedPassed()[index].startTimeText + "!";
		};

		self.loadViewModel = function (date, callback) {
			ajax.Ajax({
				url: 'Asm/Today',
				dataType: "json",
				type: 'GET',
				//pass as string to make sure no time included due to time zone stuff
				data: { asmZeroLocal: moment(date).format('YYYY-MM-DD') },
				success: callback
			});
		};

		self.loadSetting = function (callback) {
			ajax.Ajax({
				url: 'Asm/AlertTimeSetting',
				dataType: "json",
				type: 'GET',
				success: callback
			});
		};

		self._readAlertTimeSetting = function (data) {
			self.alertTimeSetting = data;
		};

		self._createLayers = function (layers) {
			var newLayers = new Array();
			var canvasPosition = self.canvasPosition();
			$.each(layers, function (key, layer) {
				newLayers.push(new activityLayer(layer, canvasPosition));
			});
			self.layers = newLayers;
			self.layersRemovedPassed();
		};
	}
	function _initNotificationViewModel() {
		var yesterdayFromNow = moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day').toDate();
		alertvm = new notificationActivities(yesterdayFromNow);
		var activityData;
		var alertSetting;

		var dataLoadDeffered = $.Deferred();
		alertvm.loadViewModel(
			yesterdayFromNow,
			function (data) {
				activityData = data.Layers;
				dataLoadDeffered.resolve();
			});
		var settingLoadDeffered = $.Deferred();
		alertvm.loadSetting(function (data) {
			alertSetting = data.SecondsBeforeChange;
			settingLoadDeffered.resolve();
		});
		$.when(dataLoadDeffered, settingLoadDeffered).done(function () {
			alertvm._createLayers(activityData);
			alertvm._readAlertTimeSetting(alertSetting);
			_startAlert();
		});
	}

	var alertTimeInterval = 0;
	var currentLayerIndex = 0;
	function _alertActivity() {
		if (currentLayerIndex < alertvm.layersRemovedPassed().length) {
			alertTimeInterval = alertvm.getTimeInterval(currentLayerIndex) * 1000;
			if (alertTimeInterval < 0) {
				currentLayerIndex++;
				alertTimeInterval = alertvm.getTimeInterval(currentLayerIndex) * 1000;
			}

			if ((alertTimeInterval <= alertvm.alertTimeSetting * 1000) && (alertTimeInterval > 0)) {
				Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage(currentLayerIndex));
			} else if (alertTimeInterval > 0) {
				setTimeout(function () {
					Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage(currentLayerIndex - 1));
					_alertActivity();
				}, alertTimeInterval);
			}
			currentLayerIndex++;
		}
	}

	function _startAlert() {
		setTimeout(_alertActivity, alertTimeInterval);
	}

	return {
		StartAlert: function (options) {
			notifyOptions = options;
			_initNotificationViewModel(options);
		}
	};
})(jQuery);