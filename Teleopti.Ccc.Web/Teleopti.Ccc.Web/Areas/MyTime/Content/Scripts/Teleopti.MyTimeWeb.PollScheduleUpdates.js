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

	function _setListener(name, callback) {
		currentListener = { name: name, callback: callback };
	};


	function _checkIfScheduleHasUpdates() {
		var deferred = $.Deferred();
		ajax.Ajax({
			url: 'Asm/CheckIfScheduleHasUpdates',
			dataType: 'json',
			type: 'POST',
			success: function (data) {
				deferred.resolve(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				deferred.reject(false);
			}
		});
		return deferred.promise();
	}

	function _showNotice() {
		var notifyText = _getNotifyText();
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
			_clearInterval();
			_setUpInterval();
		}
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
		_checkIfScheduleHasUpdates().done(function (data) {
			if (!!data && !!data.HasUpdates) {
				_showNotice();
				currentListener && currentListener.callback && currentListener.callback(settings.notifyPeriod);
			}
		});
	}

	function _getNotifyText() {
		return settings.notifyText;
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