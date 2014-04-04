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

Teleopti.MyTimeWeb.Asm = (function () {
	var refreshSeconds = 1;
	var pixelPerHours = 40;
	var timeLineMarkerWidth = 40;
	var vm;
	var alertvm;
	var notifyOptions;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var _settings;

	function resize() {
		var innerWidth = document.documentElement.clientWidth || document.body.clientWidth || window.innerWidth;
		var innerHeight = document.documentElement.clientHeight || document.body.clientHeight || window.innerHeight;
		var targetWidth = 415;
		var targetHeight = 66;
		window.resizeBy(targetWidth - innerWidth, targetHeight - innerHeight);
	}

	function asmViewModel(yesterday) {
		var self = this;
		self.intervalPointer = null;

		self.loadViewModel = function () {
			ajax.Ajax({
				url: 'Asm/Today',
				dataType: "json",
				type: 'GET',
				//pass as string to make sure no time included due to time zone stuff
				data: { asmZeroLocal: moment(self.yesterday()).format('YYYY-MM-DD') },
				success: function (data) {
					self.hours(data.Hours);
					self._createLayers(data.Layers);
					self.unreadMessageCount(data.UnreadMessageCount);
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
		self.currentLayerString = ko.computed(function () {
			var layer = self.visibleLayers()[0];
			if (typeof layer != "undefined" && layer.active()) {
				return layer.title();
			}
			return null;
		});
		self.nextLayerString = ko.computed(function () {
			var layer;
			if (self.currentLayerString() == null) {
				layer = self.visibleLayers()[0];
			} else {
				layer = self.visibleLayers()[1];
			}
			if (typeof layer != "undefined") {
				return layer.title();
			}
			return null;
		});
		self.now = ko.observable(new Date().getTeleoptiTime());
		self.yesterday = ko.observable(yesterday);
		self.unreadMessageCount = ko.observable(0);
		self.canvasPosition = ko.computed(function () {
			var msSinceStart = self.now() - self.yesterday().getTime();
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});
		self.now.subscribe(function (currentMs) {
			var yesterdayPlus2Days = moment(new Date(self.yesterday().getTime())).add('days',2).toDate();
			if (currentMs > yesterdayPlus2Days.getTime()) {
				var todayMinus1 = moment(new Date(currentMs)).add('days', -1).startOf('day').toDate();
				self.yesterday(todayMinus1);
			}
		});
		self.unreadMessages = ko.computed(function () {
			return self.unreadMessageCount() > 0;
		});
		self.yesterday.subscribe(function () {
			if (self.intervalPointer != null) {
				clearInterval(self.intervalPointer);
			}
			self.loadViewModel();
		});
		self.openMessages = function () {
			window.open(_settings.baseUrl + 'Message/Index/', 'MessageWindow');
		};
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
		self.startMinutesSinceAsmZero = layer.StartMinutesSinceAsmZero;
		self.title = ko.computed(function () {
			var nextDayAdder = '';
			if (layer.StartMinutesSinceAsmZero > 2 * 24 * 60) {
				nextDayAdder = '+1';
			}
			return layer.StartTimeText + nextDayAdder + '-' + layer.EndTimeText + ' ' + layer.Payload;
		});
		self.visible = ko.computed(function () {
			if($.isFunction(canvasPosition))
				var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition());
			else
				var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition);
			var startPos = parseFloat(self.leftPx);
			var endPos = startPos + parseFloat(self.paddingLeft);
			return endPos > timelinePosition;
		});
		self.active = ko.computed(function () {
			if (!self.visible)
				return false;
			var startPos = parseFloat(self.leftPx);
			if($.isFunction(canvasPosition))
				var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition());
			else {
				var timelinePosition = timeLineMarkerWidth - parseFloat(canvasPosition);
			}
			var isActive = startPos <= timelinePosition;
			return isActive;
		});
	}

	function _showAsm() {
		_setFixedElementAttributes();

		if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
			setTimeout(function () {
				resize();
			}, 200);
		} else {
			resize();
		}

		var yesterDayFromNow = moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day').toDate();
		vm = new asmViewModel(yesterDayFromNow);
		ko.applyBindings(vm);
		vm.loadViewModel();
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', (3 * 24 * pixelPerHours));
		$('.asm-timeline-line').css('width', (pixelPerHours));
		$('.col-1').hide(); //hide footer that takes "empty" space
	}

	function _listenForEvents(listeners) {
		ajax.Ajax({
			url: 'MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: listeners,
					domainType: 'IScheduleChangedInDefaultScenario',
					businessUnitId: data.BusinessUnitId,
					datasource: data.DataSourceName,
					referenceId: data.AgentId
				});
			}
		});
	}

	function _validSchedulePeriod(notification) {
		var messageStartDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate)).add('days', -1).toDate();
		var messageEndDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate)).add('days', 1).toDate();
		var listeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours',-1).toDate();
		var listeningEndDate = moment(new Date(listeningStartDate.getTime())).add('days', 1).toDate();

		if (messageStartDate < listeningEndDate && messageEndDate > listeningStartDate) {
			return true;
		}
		return false;
	}
    
	function _makeSureWeAreLoggedOn() {
	    var ajax = new Teleopti.MyTimeWeb.Ajax();
	    ajax.Ajax({
	        url: 'MessageBroker/FetchUserData',
	        dataType: "json",
	        async: false,
	        type: 'GET',
	        success: function () {
	            setTimeout(_makeSureWeAreLoggedOn, 20 * 60 * 1000);
	        }
	    });
	}
	///////////
	function notificationViewModel(yesterday) {
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
		self.currentLayer = function () {
			var layer = self.layersRemovedPassed()[0];
			if (typeof layer != "undefined" && layer != null && layer.active()) {
				return layer;
			}
			return null;
		};
		self.nextLayer = function () {
			var layer;
			if (self.currentLayer() == null) {
				layer = self.layersRemovedPassed()[0];
			} else {
				layer = self.layersRemovedPassed()[1];
			}
			if (typeof layer != "undefined") {
				return layer;
			}
			return null;
		};
		self.timeInterval = function (index) {
			var interval = 0;
			var length = self.layersRemovedPassed().length;
			if (index >= 0 && index < length) {
				var msSinceStart = self.now - self.yesterday.getTime();
				var secondsSinceStart = msSinceStart / 1000;
				if (self.layersRemovedPassed()[index].startMinutesSinceAsmZero * 60 - secondsSinceStart < 0)
					interval = -1;
				else if (self.layersRemovedPassed()[index].startMinutesSinceAsmZero * 60 - secondsSinceStart > self.alertTimeSetting)
					interval = self.layersRemovedPassed()[index].startMinutesSinceAsmZero * 60 - secondsSinceStart - self.alertTimeSetting;
				else {
					interval = self.alertTimeSetting - self.layersRemovedPassed()[index].startMinutesSinceAsmZero * 60 - secondsSinceStart;
				}
			}
			return interval;
		};
		
		self.alertMessage = function (index) {
			return self.layersRemovedPassed()[index].payload + "at" + self.layersRemovedPassed()[index].startTimeText + "!";
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

		self.loadSetting = function(callback) {
			ajax.Ajax({
				url: 'Asm/AlertTimeSetting',
				dataType: "json",
				type: 'GET',
				success: callback
			});
		};

		self._readAlertTimeSetting = function(data) {
			self.alertTimeSetting = data;
		};
		
		self._createLayers = function (layers) {
			var newLayers = new Array();
			var canvasPosition = self.canvasPosition();
			$.each(layers, function (key, layer) {
				newLayers.push(new layerViewModel(layer, canvasPosition));
			});
			self.layers = newLayers;
			self.layersRemovedPassed();
			self.currentLayer();
			self.nextLayer();

		};
	}
	function _initNotificationViewModel() {
		var yesterDayFromNow = moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day').toDate();
		alertvm = new notificationViewModel(yesterDayFromNow);
		var activityData;
		var alertSetting;
		
		var dataLoadDeffered = $.Deferred();
		alertvm.loadViewModel(
			yesterDayFromNow,
			function (data) {
				activityData = data.Layers;
				dataLoadDeffered.resolve();
			});
		var settingLoadDeffered = $.Deferred();
		alertvm.loadSetting(function(data) {
			alertSetting = data.SecondsBeforeChange;
			settingLoadDeffered.resolve();
		});
		$.when(dataLoadDeffered, settingLoadDeffered).done(function () {
			alertvm._createLayers(tempData);
			alertvm._readAlertTimeSetting(alertSetting);
			_startAlert();
		});
	}

	var timeInterval = 0;
	var layerIdx = 0;
	function _alertActivity() {
		if (layerIdx < alertvm.layersRemovedPassed().length) {
			timeInterval = alertvm.timeInterval(layerIdx) * 1000;
			if (timeInterval < 0) {
				layerIdx++;
				timeInterval = alertvm.timeInterval(layerIdx) * 1000;
			}

			if ((alertvm.timeInterval(layerIdx) <= alertvm.alertTimeSetting) && (alertvm.timeInterval(layerIdx) > 0)) {
				Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage(layerIdx));
			} else if (timeInterval > 0) {
				console.log("Timer:" + timeInterval);
				setTimeout(function () {
					Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, alertvm.alertMessage(layerIdx-1));
					 _alertActivity();
				}, timeInterval);
			}
			layerIdx++;
		}
		 
	}
	
	function _startAlert() {
		setTimeout(_alertActivity, timeInterval);
	}
	////////////
    function _startPollingToAvoidLogOut() {
        setTimeout(_makeSureWeAreLoggedOn, 20 * 60 * 1000);
    }
	
	return {
		ShowAsm: function (settings) {
			_settings = settings;
			_showAsm();
		    _startPollingToAvoidLogOut();
		},
		ListenForScheduleChanges: function (options, eventListeners) {
			notifyOptions = options;
			_listenForEvents(eventListeners);
		},
		NotifyWhenScheduleChangedListener: function (notification) {
			if (_validSchedulePeriod(notification)) {
			    var startDate = new moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate)).format('L');
				var notifyText = notifyOptions.notifyText.format(startDate);
				Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, notifyText);
			}
		},
		ReloadAsmViewModelListener: function (notification) {
			if (_validSchedulePeriod(notification)) {
				vm.loadViewModel();
			}
		},
		SetMessageCount: function (data) {
			if (vm) {
				vm.unreadMessageCount(data.UnreadMessagesCount);				
			}
		},
		MakeSureWeAreLoggedOn: _makeSureWeAreLoggedOn,
		StartAlert: function (options) {
			notifyOptions = options;
			_initNotificationViewModel(options);
		}
	};
})(jQuery);