Teleopti.MyTimeWeb.AlertActivity = (function () {
	var alertvm;
	var notifyOptions = {};
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var currentTimeout = null;
	var notificationDisplayTime = null;
	var notificationDisplayTimeRequest = null;
	
	function loadBeforeAlertTimeSetting(callback) {
		ajax.Ajax({
			url: 'Asm/AlertTimeSetting',
			dataType: "json",
			type: 'GET',
			success: callback
		});
	}

	function loadAlertNotificationDisplayingTimeSetting(callback) {
		if (!notificationDisplayTimeRequest) {
			notificationDisplayTimeRequest = ajax.Ajax({
				url: 'Asm/NotificationsTimeToStaySetting',
				dataType: 'json',
				type: 'GET',
				success: function(displayTime) {
					notificationDisplayTime = displayTime.DurationInSecond;
					Teleopti.MyTimeWeb.Asm.UpdateNotificationDisplayTimeSetting(displayTime.DurationInSecond);
					notifyOptions.timeout = displayTime.DurationInSecond * 1000;
				}
			});
		}
		notificationDisplayTimeRequest.done(function() { callback(notificationDisplayTime); });
	}

	function activityLayer(layer) {
		var self = this;

		self.activityName = layer.Payload;
		self.startTimeText = layer.StartTimeText;
		self.startMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero;
		self.endTimeText = layer.EndTimeText;
		self.endMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero + layer.LengthInMinutes;
		self.isIdleLayer = function () {
			return self.activityName === "-=IDLE=-";
		};
	}

	function notificationActivities() {

		var self = this;
		self.getCurrentTime = function() {
			return new Date().getTeleoptiTimeInUserTimezone();
		};
		self.timeZero = moment(self.getCurrentTime()).add('days', -1).startOf('day').toDate();

		self.alertTimeSetting = 60; //default setting 60secs
		self.alertMessage = "";
		self.restartAlertDelayTime = 60;
		self.layers = [];

		self.getCurrentLayerIndex = function () {
			var now = (self.getCurrentTime() - self.timeZero) / 1000;

			var layerCount = self.layers.length;

			var shiftStartTime = self.layers[0].startMinutesSinceAsmZero * 60;
			var shiftEndTime = self.layers[layerCount - 1].endMinutesSinceAsmZero * 60;

			var currentLayer = -1;
			if (now < shiftStartTime) {
				// First activity not started;
				currentLayer = -1;
			} else if (now >= shiftEndTime) {
				// Last activity already finished;
				currentLayer = layerCount;
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
			if (layerCount === 0) {
				// No shift scheduled today
				return {
					message: "",
					timespan: -1
				};
			}

			var layerIndex = self.getCurrentLayerIndex();
			var secondsSinceStart = (self.getCurrentTime() - self.timeZero) / 1000;
			
			var layer;
			var activityName, alertMessage;
			var timeDiff;

			if (layerIndex < 0) {
				// First activity not started
				layer = self.layers[0];
				activityName = layer.activityName;
				alertMessage = notifyOptions.comingMessageTemplate 
					? notifyOptions.comingMessageTemplate.format(activityName, layer.startTimeText)
					: "";

				var shiftStartTime = layer.startMinutesSinceAsmZero * 60;
				timeDiff = shiftStartTime - secondsSinceStart;
			} else if (layerIndex >= 0 && layerIndex < layerCount - 1) {
				// The shift is passing...
				if (self.layers[layerIndex + 1].isIdleLayer()) {
					var currentLayer = self.layers[layerIndex];
					alertMessage = notifyOptions.endingMessageTemplate
						? notifyOptions.endingMessageTemplate.format(currentLayer.endTimeText)
						: "";

					var shiftEndTime = currentLayer.endMinutesSinceAsmZero * 60;
					timeDiff = shiftEndTime - secondsSinceStart;
				} else {
					var nextLayer = self.layers[layerIndex + 1];
					activityName = nextLayer.activityName;
					alertMessage = notifyOptions.comingMessageTemplate 
						? notifyOptions.comingMessageTemplate.format(activityName, nextLayer.startTimeText)
						: "";

					var nextActivityStartTime = nextLayer.startMinutesSinceAsmZero * 60;
					timeDiff = nextActivityStartTime - secondsSinceStart;
				}
			} else if (layerIndex === (layerCount - 1)) {
				// Now is in latest activity
				currentLayer = self.layers[layerIndex];
				alertMessage = notifyOptions.endingMessageTemplate 
					? notifyOptions.endingMessageTemplate.format(currentLayer.endTimeText)
					: "";

				shiftEndTime = currentLayer.endMinutesSinceAsmZero * 60;
				timeDiff = shiftEndTime - secondsSinceStart;
			} else {
				// Entire shift already finished!
				alertMessage = "";
				timeDiff = -1;
			}

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
				data: { asmZeroLocal: Teleopti.MyTimeWeb.Common.FormatServiceDate(date) },
				success: callback
			});
		};

		self._readAlertTimeSetting = function (data) {
			self.alertTimeSetting = data;
		};

		self._createLayers = function (layers) {
			self.layers = [];

			if (layers.length === 0) {
				return;
			}

			var previousLayerEndTime = layers[0].StartMinutesSinceAsmZero + layers[0].LengthInMinutes;
			for (var i = 0; i < layers.length; i++) {
				var currentLayer = layers[i];
				var currentLayerStartTime = currentLayer.StartMinutesSinceAsmZero;

				if (i > 0 && currentLayerStartTime > previousLayerEndTime) {
					// Add fake idle layer for time interval between 2 shifts
					var idleLayer = new activityLayer({
						Payload: '-=IDLE=-',
						StartTimeText: layers[i - 1].EndTimeText,
						EndTimeText: currentLayer.StartTimeText,
						StartMinutesSinceAsmZero: previousLayerEndTime,
						LengthInMinutes: currentLayerStartTime - previousLayerEndTime
					});
					self.layers.push(idleLayer);
				}
				previousLayerEndTime = currentLayer.StartMinutesSinceAsmZero + currentLayer.LengthInMinutes;
				var newLayer = new activityLayer(currentLayer);
				self.layers.push(newLayer);
			}
		};
	}

	function initNotificationViewModel() {
		alertvm = new notificationActivities();

		var activityData,
			beforeAlertTimeSetting;

		var dataLoadDeffered = $.Deferred();
		alertvm.loadViewModel(
			alertvm.timeZero,
			function (data) {
				activityData = data.Layers;
				dataLoadDeffered.resolve();
			});
		var settingLoadDeffered = $.Deferred();
		loadBeforeAlertTimeSetting(function (data) {
			beforeAlertTimeSetting = data.SecondsBeforeChange;
			settingLoadDeffered.resolve();
		});

		loadAlertNotificationDisplayingTimeSetting(function() {});

		$.when(dataLoadDeffered, settingLoadDeffered).done(function () {
			alertvm._createLayers(activityData);
			alertvm._readAlertTimeSetting(beforeAlertTimeSetting);
			startAlert();
		});
	}

	function alertActivity() {
		Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage);

		if (currentTimeout !== null) clearTimeout(currentTimeout);
		currentTimeout = setTimeout(startAlert, alertvm.restartAlertDelayTime * 1000);
	}

	function startAlert() {
		var interval = 0;
		var delayTime = alertvm.alertTimeSetting;

		var alert = alertvm.getCurrentAlert();
		if (alert.timespan >= 0) {
			if (alert.timespan >= alertvm.alertTimeSetting) {
				interval = alert.timespan - alertvm.alertTimeSetting;
				delayTime = alertvm.alertTimeSetting + 5;
			}
			if (alert.timespan < alertvm.alertTimeSetting) {
				interval = 0;
				delayTime = alert.timespan + 5;
			}

			alertvm.alertMessage = alert.message;
			alertvm.restartAlertDelayTime = delayTime;

			if (currentTimeout !== null) clearTimeout(currentTimeout);
			currentTimeout = setTimeout(alertActivity, interval * 1000);
		} else {
			// timespan in negative number means no schedule today or all activity finished.
			// Then re-start alert when tomorrow comes.
			var timeZeroTomorrow = moment(alertvm.getCurrentTime()).add('days', 1).startOf('day').toDate();
			var intervalForTomorrowInSecond = (timeZeroTomorrow - alertvm.getCurrentTime()) / 1000;

			if (currentTimeout !== null) clearTimeout(currentTimeout);
			currentTimeout = setTimeout(function() {
				initNotificationViewModel();
			}, intervalForTomorrowInSecond * 1000);
		}
	}

	return {
		StartAlertNextActivity: function (notification) {
			notifyOptions = notification;
			initNotificationViewModel();
		},
		RefreshAlertNextActivity: function () {
			if (notifyOptions) {
				initNotificationViewModel();
			}			
		},
		GetNotificationDisplayTime:loadAlertNotificationDisplayingTimeSetting,
		AbortAjax: function () {
			if (currentTimeout !== null) clearTimeout(currentTimeout);
			ajax.AbortAll();
		},
		_replaceAjax: function(another_ajax) {
			ajax = another_ajax;
		}
	};
})(jQuery);