/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Notifier.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/noty/jquery.noty.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AlertActivity = (function () {
	var pixelPerHours = 40;
	var alertvm;
	var notifyOptions;
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	function activityLayer(layer) {
		var self = this;

		self.activityName = layer.Payload;
		self.startTimeText = layer.StartTimeText;
		self.startMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero;
		self.endTimeText = layer.EndTimeText;
		self.endMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero + layer.LengthInMinutes;
	}

	function notificationActivities(yesterdayZero) {
		var self = this;
		self.now = new Date().getTeleoptiTime();
		self.yesterday = yesterdayZero;
		self.alertTimeSetting = 60; //default setting 60secs
		self.alertMessage = "";
		self.restartAlertDelayTime = 60;
		self.canvasPosition = function () {
			var msSinceStart = self.now - self.yesterday.getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		};
		self.layers = new Array();

		self.getCurrentLayerIndex = function () {
			var now = (self.now - self.yesterday.getTime()) / 1000;
			var layerCount = self.layers.length;

			var shiftStartTime = self.layers[0].startMinutesSinceAsmZero * 60;
			var shiftEndTime = self.layers[layerCount - 1].endMinutesSinceAsmZero * 60;
			
			var currentLayer = -1;
			if (now < shiftStartTime) {
				// First activity not started;
				currentLayer = - 1;
			} else if (now >= shiftEndTime) {
				// Last activity already finished;
				currentLayer = layerCount + 1;
			} else {
				for (var i = 0; i < layerCount; i++) {
					var layer = self.layers[i];
					var layerStartTime = layer.startMinutesSinceAsmZero * 60;
					var layerEndTime = layer.endMinutesSinceAsmZero * 60;

					if (now >= layerStartTime && now < layerEndTime) {
						currentLayer = i;
						break;
					}
				}
			}

			return currentLayer;
		};

		self.getCurrentAlert = function () {
			var layerCount = self.layers.length;
			Teleopti.MyTimeWeb.Test.TestMessage("layerCount: " + layerCount);

			if (layerCount == 0) {
				Teleopti.MyTimeWeb.Test.TestMessage("No layer found");
				// No shift scheduled today
				return {
					message: "",
					timespan: -1
				};
			}

			var layerIndex = self.getCurrentLayerIndex();
			var secondsSinceStart = (self.now - self.yesterday.getTime()) / 1000;

			Teleopti.MyTimeWeb.Test.TestMessage("Current Layer Index: " + layerIndex);

			var layer;
			var activityName, alertMessage;
			var timeDiff;
			if (layerIndex < 0) {
				// First activity not started
				layer = self.layers[0];
				activityName = layer.activityName;
				alertMessage = notifyOptions.comingMessageTemplate.format(activityName, layer.startTimeText);

				var shiftStartTime = layer.startMinutesSinceAsmZero * 60;
				timeDiff = shiftStartTime - secondsSinceStart;
				Teleopti.MyTimeWeb.Test.TestMessage("First activity not started");
			} else if (layerIndex >= 0 && layerIndex < layerCount - 1) {
				// The shift is passing...
				layer = self.layers[layerIndex + 1];
				activityName = layer.activityName;
				alertMessage = notifyOptions.comingMessageTemplate.format(activityName, layer.startTimeText);

				var nextActivityStartTime = layer.startMinutesSinceAsmZero * 60;
				timeDiff = nextActivityStartTime - secondsSinceStart;
				Teleopti.MyTimeWeb.Test.TestMessage("The shift is passing...");
			} else if (layerIndex == layerCount - 1) {
				// Now is in latest activity
				layer = self.layers[layerCount - 1];
				activityName = layer.activityName;
				alertMessage = notifyOptions.endingMessageTemplate.format(activityName, layer.endTimeText);

				var shiftEndTime = self.layers[layerCount - 1].endMinutesSinceAsmZero * 60;
				timeDiff = shiftEndTime - secondsSinceStart;
				Teleopti.MyTimeWeb.Test.TestMessage("Now is in latest activity...");
			} else {
				// Entire shift already finished!
				alertMessage = "";
				timeDiff = -1;
				Teleopti.MyTimeWeb.Test.TestMessage("Entire shift already finished!");
			}

			Teleopti.MyTimeWeb.Test.TestMessage("alert message: " + alertMessage);
			Teleopti.MyTimeWeb.Test.TestMessage("timeDiff: " + timeDiff);

			return {
				message: alertMessage, 
				timespan: timeDiff
			};
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
			Teleopti.MyTimeWeb.Test.TestMessage("_createLayers: ");
			var newLayers = new Array();
			var canvasPosition = self.canvasPosition();
			$.each(layers, function (key, layer) {
				newLayers.push(new activityLayer(layer, canvasPosition));
			});
			self.layers = newLayers;
		};
	}

	function initNotificationViewModel() {
		Teleopti.MyTimeWeb.Test.TestMessage("initNotificationViewModel");
		var yesterdayZero = moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day').toDate();

		alertvm = new notificationActivities(yesterdayZero);

		var activityData;
		var alertSetting;

		var dataLoadDeffered = $.Deferred();
		alertvm.loadViewModel(
			yesterdayZero,
			function (data) {
				activityData = data.Layers;
				dataLoadDeffered.resolve();
			});
		var settingLoadDeffered = $.Deferred();
		alertvm.loadSetting(function (data) {
			Teleopti.MyTimeWeb.Test.TestMessage("loading setting...");
			alertSetting = data.SecondsBeforeChange;
			settingLoadDeffered.resolve();
		});
		$.when(dataLoadDeffered, settingLoadDeffered).done(function () {
			Teleopti.MyTimeWeb.Test.TestMessage("initNotificationViewModel.callback");
			alertvm._createLayers(activityData);
			alertvm._readAlertTimeSetting(alertSetting);
			startAlert();
			Teleopti.MyTimeWeb.Test.TestMessage("/initNotificationViewModel.callback");
		});
		Teleopti.MyTimeWeb.Test.TestMessage("/initNotificationViewModel");
	}

	function alertActivity() {
		Teleopti.MyTimeWeb.Test.TestMessage("alertActivity");

		Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage);

		// Restart alert after delayTime.
		setTimeout(startAlert, alertvm.restartAlertDelayTime * 1000);
		Teleopti.MyTimeWeb.Test.TestMessage("/alertActivity");
	}

	function startAlert() {
		Teleopti.MyTimeWeb.Test.TestMessage("startAlert");
		var alert = alertvm.getCurrentAlert();
		var interval = 0;
		var delayTime = alertvm.alertTimeSetting;
		Teleopti.MyTimeWeb.Test.TestMessage("alertvm.alertTimeSetting: " + alertvm.alertTimeSetting);

		// timespan in negative number means no schedule today or all activity finished
		if (alert.timespan >= 0) {
			Teleopti.MyTimeWeb.Test.TestMessage("startAlert!!");
			if (alert.timespan >= alertvm.alertTimeSetting) {
				Teleopti.MyTimeWeb.Test.TestMessage("alert.timespan >= alertvm.alertTimeSetting");
				interval = alert.timespan - alertvm.alertTimeSetting;
				delayTime = alertvm.alertTimeSetting;
			}
			if (alert.timespan >= 0 && alert.timespan < alertvm.alertTimeSetting) {
				Teleopti.MyTimeWeb.Test.TestMessage("alert.timespan >= 0 && alert.timespan < alertvm.alertTimeSetting");
				interval = 0;
				delayTime = alert.timespan == 0 ? 1 : alert.timespan;
			}

			Teleopti.MyTimeWeb.Test.TestMessage("interval: " + interval);
			Teleopti.MyTimeWeb.Test.TestMessage("delayTime: " + delayTime);

			alertvm.alertMessage = alert.message;
			alertvm.restartAlertDelayTime = delayTime;

			setTimeout(alertActivity, interval * 1000);
		}
		Teleopti.MyTimeWeb.Test.TestMessage("/startAlert");
	}

	return {
		StartAlert: function (options) {
			Teleopti.MyTimeWeb.Test.GetTestMessages();
			Teleopti.MyTimeWeb.Test.TestMessage("StartAlert");
			notifyOptions = options;
			initNotificationViewModel(options);
			Teleopti.MyTimeWeb.Test.TestMessage("/StartAlert");
		}
	};
})(jQuery);