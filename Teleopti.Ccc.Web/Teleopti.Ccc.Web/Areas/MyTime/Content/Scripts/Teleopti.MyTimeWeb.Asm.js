if (typeof Teleopti === 'undefined') {
	Teleopti = {};
	if (typeof Teleopti.MyTimeWeb === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	var refreshSeconds = 5;
	var pixelPerHours = 40;
	var timeLineMarkerWidth = 40;
	var vm;
	var notifyOptions = {};
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var _settings;
	var userTimeZoneMinuteOffset = 0;
	var currentPage = 'Teleopti.MyTimeWeb.Asm';
	var _notificationTrackIdList = [];

	function resize() {
		var innerWidth = document.documentElement.clientWidth || document.body.clientWidth || window.innerWidth;
		var innerHeight = document.documentElement.clientHeight || document.body.clientHeight || window.innerHeight;
		var targetWidth = 415;
		var targetHeight = 66;
		window.resizeBy(targetWidth - innerWidth, targetHeight - innerHeight);
	}

	function getUtcNowString() {
		var now = new Date(new Date().getTeleoptiTime());

		var dateList = [
			now.getUTCFullYear().toString(),
			(now.getUTCMonth() + 1).toString(),
			now.getUTCDate().toString(),
			now.getUTCHours().toString(),
			now.getUTCMinutes().toString(),
			now.getUTCSeconds().toString()
		];

		dateList.forEach(function (item, i) {
			if (item.toString().length == 1) {
				dateList[i] = '0' + dateList[i];
			}
		});

		dateStr =
			dateList[0] +
			'-' +
			dateList[1] +
			'-' +
			dateList[2] +
			'T' +
			dateList[3] +
			':' +
			dateList[4] +
			':' +
			dateList[5] +
			'Z';

		return dateStr;
	}

	function asmViewModel(yesterday, enableIntervalUpdate) {
		var self = this;
		self.intervalPointer = null;

		function setCurrentTime() {
			self.now(
				Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone(getUtcNowString()).add(
					userTimeZoneMinuteOffset,
					'minute'
				)
			);
		}

		self.loadViewModel = function () {
			// Clear existed interval to prevent duplicate invoke to setCurrentTime()
			if (self.intervalPointer !== null) {
				clearInterval(self.intervalPointer);
			}

			ajax.Ajax({
				url: 'Asm/Today',
				dataType: 'json',
				type: 'GET',
				//pass as string to make sure no time included due to time zone stuff
				data: { asmZeroLocal: Teleopti.MyTimeWeb.Common.FormatServiceDate(self.yesterday()) },
				success: function (data) {
					self.hours(data.Hours);
					self._createLayers(data.Layers);
					self.unreadMessageCount(data.UnreadMessageCount);
					$('.asm-outer-canvas').show();
					userTimeZoneMinuteOffset = data.UserTimeZoneMinuteOffset;
					setCurrentTime();

					if (enableIntervalUpdate) {
						self.intervalPointer = setInterval(function () {
							setCurrentTime();
						}, 1000 * refreshSeconds);
					}
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

		self.currentLayerString = ko.observable(null);
		self.nextLayerString = ko.observable(null);

		self.hours = ko.observableArray();
		self.layers = ko.observableArray();
		self.visibleLayers = ko.observableArray();

		self.visibleLayers.subscribe(function (newVisualLayers) {
			var currentLayer = newVisualLayers[0];
			var currentLayerDesc = currentLayer != undefined && currentLayer.active() ? currentLayer.title() : null;

			var nextLayer = currentLayerDesc == null ? newVisualLayers[0] : newVisualLayers[1];
			var nextLayerDesc = nextLayer != undefined ? nextLayer.title() : null;

			self.currentLayerString(currentLayerDesc);
			self.nextLayerString(nextLayerDesc);
		});

		self.now = ko.observable(moment(new Date().getTeleoptiTimeInUserTimezone()));
		self.yesterday = ko.observable(yesterday);
		self.unreadMessageCount = ko.observable(0);
		self.canvasPosition = ko.computed(function () {
			var msSinceStart =
				(self.now().hour() * 60 * 60 + self.now().minute() * 60 + self.now().second() + 24 * 60 * 60) * 1000;
			var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
			return -(pixelPerHours * hoursSinceStart) + 'px';
		});

		self.now.subscribe(function (currentMs) {
			var yesterdayPlus2Days = moment(new Date(self.yesterday().getTime())).add('days', 2);
			var nowString =
				currentMs.year() +
				'-' +
				(currentMs.month() + 1) +
				'-' +
				currentMs.date() +
				' ' +
				currentMs.hour() +
				':' +
				currentMs.minute() +
				':' +
				currentMs.second();

			if (moment(nowString) > yesterdayPlus2Days) {
				var todayMinus1 = moment(new Date(currentMs))
					.add('days', -1)
					.startOf('day')
					.toDate();
				self.yesterday(todayMinus1);
			}

			var visibleLayers = $.grep(self.layers(), function (n, i) {
				return n.visible();
			});

			self.visibleLayers(visibleLayers);
		});
		self.unreadMessages = ko.computed(function () {
			return self.unreadMessageCount() > 0;
		});
		self.yesterday.subscribe(function () {
			self.loadViewModel();
		});
		self.openMessages = function () {
			window.open(_settings.baseUrl + 'Message/Index/', 'MessageWindow');
		};
	}

	function layerViewModel(layer, parent) {
		var self = this;

		self.leftPx = (layer.StartMinutesSinceAsmZero * pixelPerHours) / 60 + timeLineMarkerWidth + 'px';
		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.paddingLeft = (layer.LengthInMinutes * pixelPerHours) / 60 + 'px';
		self.startTimeText = layer.StartTimeText;
		self.endTimeText = layer.EndTimeText;
		self.title = ko.computed(function () {
			var nextDayAdder = '';
			if (layer.StartMinutesSinceAsmZero > 2 * 24 * 60) {
				nextDayAdder = '+1';
			}
			return layer.StartTimeText + nextDayAdder + '-' + layer.EndTimeText + ' ' + layer.Payload;
		});
		self.visible = ko.computed(function () {
			var timelinePosition = timeLineMarkerWidth - parseFloat(parent.canvasPosition());
			var startPos = parseFloat(self.leftPx);
			var endPos = startPos + parseFloat(self.paddingLeft);
			return endPos > timelinePosition;
		});
		self.active = ko.computed(function () {
			if (!self.visible) return false;
			var startPos = parseFloat(self.leftPx);
			var timelinePosition = timeLineMarkerWidth - parseFloat(parent.canvasPosition());
			var isActive = startPos <= timelinePosition;
			return isActive;
		});
	}

	function _showAsm(enableIntervalUpdate) {
		_setFixedElementAttributes();

		if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
			setTimeout(function () {
				resize();
			}, 200);
		} else {
			resize();
		}

		var yesterDayFromNow = moment(new Date(new Date().getTeleoptiTimeInUserTimezone()))
			.add('days', -1)
			.startOf('day')
			.toDate();
		vm = new asmViewModel(yesterDayFromNow, enableIntervalUpdate);
		var elementToBind = $('.asm-outer-canvas')[0];
		ko.applyBindings(vm, elementToBind);
		vm.loadViewModel();

		Teleopti.MyTimeWeb.PollScheduleUpdates.AddListener('MyTimeAsm', function (period) {
			if (_validSchedulePeriod(period, true)) {
				vm.loadViewModel();
			}
		});
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', 3 * 24 * pixelPerHours);
		$('.asm-timeline-line').css('width', pixelPerHours);
		$('.col-1').hide(); //hide footer that takes "empty" space
	}

	function _updateNotificationDisplayTimeSetting(displayTime) {
		for (var type in notifyOptions) {
			notifyOptions[type].timeout = displayTime * 1000;
		}
	}

	function _listenForScheduleChanges(options, eventListeners, domainType) {
		Teleopti.MyTimeWeb.AlertActivity.GetNotificationDisplayTime(function (displayTime) {
			notifyOptions[domainType] = options;
			notifyOptions[domainType].timeout = displayTime * 1000;
			Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
				successCallback: eventListeners,
				domainType: domainType,
				page: currentPage
			});

			Teleopti.MyTimeWeb.PollScheduleUpdates.AddListener('MyTimeAsmForScheduleChanges', function (period) {
				if (_validSchedulePeriod(period, true)) {
					eventListeners && eventListeners();
				}
			});
		});
	}

	function _validSchedulePeriod(notification, isPeriod) {
		var messageStartDate = isPeriod
			? notification.startDate
			: moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate))
				.add('days', -1)
				.toDate();
		var messageEndDate = isPeriod
			? notification.endDate
			: moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate))
				.add('days', 1)
				.toDate();
		var listeningStartDate = moment(new Date(new Date().getTeleoptiTime()))
			.add('hours', -1)
			.toDate();
		var listeningEndDate = moment(new Date(listeningStartDate.getTime()))
			.add('days', 1)
			.toDate();
		if (messageStartDate < listeningEndDate && messageEndDate > listeningStartDate) {
			return true;
		}
		return false;
	}

	function _validateNotificationSource(notification) {
		if (notification.TrackId != null && _notificationTrackIdList.indexOf(notification.TrackId) > -1) {
			return false;
		}

		_notificationTrackIdList.push(notification.TrackId);
		return true;
	}

	function _makeSureWeAreLoggedOn() {
		ajax.Ajax({
			url: 'UserData/FetchUserData',
			dataType: 'json',
			async: false,
			type: 'GET',
			success: function () {
				setTimeout(_makeSureWeAreLoggedOn, 20 * 60 * 1000);
			}
		});
	}

	function _startPollingToAvoidLogOut() {
		setTimeout(_makeSureWeAreLoggedOn, 20 * 60 * 1000);
	}

	return {
		ShowAsm: function (settings, enableIntervalUpdate) {
			Teleopti.MyTimeWeb.Common.Init(settings, ajax);
			_settings = settings;
			_showAsm(enableIntervalUpdate);
			_startPollingToAvoidLogOut();
		},
		ListenForScheduleChanges: _listenForScheduleChanges,
		UpdateNotificationDisplayTimeSetting: _updateNotificationDisplayTimeSetting,
		NotifyWhenScheduleChangedListener: function (notification, skipValidSchedulePeriod) {
			if (!notification) return;
			var shouldValid = skipValidSchedulePeriod ? true : _validSchedulePeriod(notification);
			if (shouldValid && _validateNotificationSource(notification)) {
				var changedDateRange = new moment(
					Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate)
				).format('L');
				if (notification.StartDate !== notification.EndDate) {
					changedDateRange =
						changedDateRange +
						' - ' +
						new moment(
							Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate)
						).format('L');
				}
				var notifyText = notifyOptions[notification.DomainType].notifyText.format(changedDateRange);
				Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions[notification.DomainType], notifyText);
			}
		},
		ReloadAsmViewModelListener: function (notification) {
			if (!notification) return;
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
		Dispose: function () {
			ajax.AbortAll();
			Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
		},
		_replaceAjax: function (another_ajax) {
			ajax = another_ajax;
		},
		Vm: function () {
			return vm;
		}
	};
})(jQuery);
