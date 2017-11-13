/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Notifier.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.PollScheduleUpdates = (function ($) {
	var interval;
	var listeners = [];
	var settings = null;
	var notificerDisplayTime = 0;
	var ajax = null;
	var currentListener;

	function _addListener(name, period, callback) {
		var isExists = listeners.map(function (listener) {
			return listener.name === name;
		}).length > 0;
		if (isExists)
			return;
		listeners.push({ name: name, period: period, callback: callback });
	};

	function _checkIfScheduleHasUpdates(period) {
		var startDate = period.startDate;
		var endDate = period.endDate;
		var deferred = $.Deferred();
		ajax.Ajax({
			url: 'Asm/CheckIfScheduleHasUpdates',
			dataType: 'json',
			type: 'GET',
			success: function (data) {
				deferred.resolve(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				deferred.reject(false);
			}
		});
		return deferred.promise();
	}

	function _showNotice(period) {
		var startDate = period.startDate;
		var endDate = period.endDate;
		var isValid = _validSchedulePeriod(startDate, endDate);
		if (isValid) {
			var notifyText = _getNotifyText(startDate, endDate);
			if (!notificerDisplayTime) {
				Teleopti.MyTimeWeb.AlertActivity.GetNotificationDisplayTime(function (displayTime) {
					settings.timeout = notificerDisplayTime * 1000;
					Teleopti.MyTimeWeb.Notifier.Notify(settings, notifyText);
				});
				return;
			}

			Teleopti.MyTimeWeb.Notifier.Notify(settings, notifyText);
		}
	}

	function _getNotifyText(startDate, endDate) {
		var changedDateRange = new moment(startDate).format('L');
		if (startDate !== endDate) {
			changedDateRange = changedDateRange + ' - ' + new moment(endDate).format('L');
		}
		return settings.notifyText.format(changedDateRange);
	}

	function _resetIntervalWhileGetMessageFromBroker() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: _resetInterval,
			domainType: "IScheduleChangedInDefaultScenario",
			page: "Teleopti.MyTimeWeb.PollSchedueUpdates"
		});
	}

	function _validSchedulePeriod(startDate, endDate) {
		var messageStartDate = moment(startDate).add('days', -1).toDate();
		var messageEndDate = moment(endDate).add('days', 1).toDate();
		var listeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours', -1).toDate();
		var listeningEndDate = moment(new Date(listeningStartDate.getTime())).add('days', 1).toDate();

		if (messageStartDate < listeningEndDate && messageEndDate > listeningStartDate) {
			return true;
		}
		return false;
	}

	function _clearInterval() {
		if (!interval) {
			return;
		}
		clearInterval(interval);
	}

	function _resetInterval(notification) {
		//TODO: not sure whether we need to validate if the notification date is in the period
		// if we need to validate this, then we need set up interval per listener
		_clearInterval();
		_setUpInterval();
	}

	function _setUpInterval() {
		if (settings.intervalTimeout === 0) {
			_handleListenersCallback();
			return;
		}
		interval = setInterval(function () {
			_handleListenersCallback();
		}, settings.intervalTimeout);
	}

	function _handleListenersCallback() {
		_checkIfScheduleHasUpdates(settings.notifyPeriod).done(function (data) {
			_showNotice(settings.notifyPeriod);
		});

		listeners.forEach(function (listener) {
			var curPeriod = $.isFunction(listener.period) ? listener.period() : listener.period;
			_checkIfScheduleHasUpdates(curPeriod).done(function (data) {
				if (data.HasUpdates) {
					listener.callback();
				}
			});
		});
	}

	function _init(options) {
		ajax = new Teleopti.MyTimeWeb.Ajax();
		settings = $.extend({ intervalTimeout: 5 * 60 * 1000 }, options);

		var noticeListeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours', -1).toDate();
		settings.notifyPeriod = {
			startDate: noticeListeningStartDate,
			endDate: moment(new Date(noticeListeningStartDate.getTime())).add('days', 1).toDate()
		}

		_setUpInterval();
		_resetIntervalWhileGetMessageFromBroker();
	}

	function _destroy() {
		_clearInterval();
	}

	return {
		AddListener: _addListener,
		Init: _init,
		Destroy: _destroy,
		GetNotifyText: _getNotifyText,
		GetSettings: function () {
			return settings;
		}
	};
})(jQuery);