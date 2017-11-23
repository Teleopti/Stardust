/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Notifier.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>

Teleopti.MyTimeWeb.PollScheduleUpdates = (function ($) {
	var interval;
	var currentListener;
	var settings = null;
	var notificerDisplayTime = 0;
	var ajax = null;
	var currentListener;

	function _setListener(name, period, callback) {
		currentListener = { name: name, period: period, callback: callback };
	};

	

	function _checkIfScheduleHasUpdates(listenerPeriod, notifyPeriod) {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: 'Asm/CheckIfScheduleHasUpdates',
			dataType: 'json',
			type: 'POST',
			data: {
				listenerPeriod: !!listenerPeriod ? _getFormatedPeriod(listenerPeriod) : undefined,
				notifyPeriod: _getFormatedPeriod(notifyPeriod)
			},
			success: function (data) {
				deferred.resolve(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				deferred.reject(false);
			}
		});
		return deferred.promise();
	}

	function _getFormatedPeriod(period) {
		return {
			StartDateTime: moment(period.startDate).format('YYYY-MM-DD'),
			EndDateTime: moment(period.endDate).format('YYYY-MM-DD')
		};
	}

	function _showNotice(period) {
		var notifyText = _getNotifyText(period);
		if (!notificerDisplayTime) {
			Teleopti.MyTimeWeb.AlertActivity.GetNotificationDisplayTime(function (displayTime) {
				notificerDisplayTime = displayTime;
				settings.timeout = displayTime * 1000;
				Teleopti.MyTimeWeb.Notifier.Notify(settings, notifyText);
			});
			return;
		}
		Teleopti.MyTimeWeb.Notifier.Notify(settings, notifyText);
	}

	function _getNotifyText(period) {
		var startDate = period.startDate;
		var endDate = period.endDate;
		var changedDateRange = new moment(startDate).format('L');
		if (startDate !== endDate) {
			changedDateRange = changedDateRange + ' - ' + new moment(endDate).format('L');
		}
		return settings.notifyText.format(changedDateRange);
	}

	function _subscribeToMessageBroker() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: _resetInterval,
			domainType: "IScheduleChangedInDefaultScenario",
			page: "Teleopti.MyTimeWeb.PollSchedueUpdates"
		});
	}

	function _clearInterval() {
		if (!interval) return;
		clearInterval(interval);
	}

	function _resetInterval(notification) {
		var notificationStartDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate)).toDate();
		var notificationEndDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate)).toDate();
		var notifyPeriodStartDate = moment(moment(settings.notifyPeriod.startDate).format('YYYY-MM-DD')).toDate();
		var notifyPeriodEndDate = moment(moment(settings.notifyPeriod.endDate).format('YYYY-MM-DD')).toDate();
		if (notificationStartDate <= notifyPeriodEndDate
			&& notificationEndDate >= notifyPeriodStartDate) {
			_resetPoller();
			_clearInterval();
			_setUpInterval();
		}
	}

	function _resetPoller() {
		ajax.Ajax({
			url: 'Asm/ResetPolling',
			dataType: 'json',
			type: 'GET'
		});
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
		var listenerPeriod
		if (currentListener) {
			listenerPeriod = $.isFunction(currentListener.period) ? currentListener.period() : currentListener.period;
		}
		_checkIfScheduleHasUpdates(listenerPeriod, settings.notifyPeriod).done(function (data) {
			if (!!data.Notify && data.Notify.length) {
				$.each(data.Notify, function (i, period) {
					_showNotice(period);
				});
			}
			if (!!data.Listener && !!data.Listener.length) {
				currentListener && currentListener.callback && currentListener.callback(data.Listener);
			}
		});
	}
	
	function _init(options) {
		ajax = new Teleopti.MyTimeWeb.Ajax();
		settings = $.extend({
			intervalTimeout: 1000 * 60 * 5
		}, options);

		var noticeListeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours', -1).toDate();
		settings.notifyPeriod = {
			startDate: noticeListeningStartDate,
			endDate: moment(new Date(noticeListeningStartDate.getTime())).add('days', 1).toDate()
		}
		_setUpInterval();
		_subscribeToMessageBroker();
	}

	function _destroy() {
		_clearInterval();
	}

	return {
		SetListener: _setListener,
		Init: _init,
		Destroy: _destroy,
		GetNotifyText: _getNotifyText,
		GetSettings: function () {
			return settings;
		}
	};
})(jQuery);