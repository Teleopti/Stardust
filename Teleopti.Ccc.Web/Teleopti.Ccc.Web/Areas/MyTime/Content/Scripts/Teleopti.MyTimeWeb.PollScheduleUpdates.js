﻿/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
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
		if (_validNotificationPeriod(notification)) {
			_clearInterval();
			_setUpInterval();
		}
	}

	function _validNotificationPeriod(notification) {
		var messageStartDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate)).add('days', -1).toDate();
		var messageEndDate = moment(Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate)).add('days', 1).toDate();
		var notifyPeriod = _getNotifyPeriod();
		var listeningStartDate = notifyPeriod.startDate;
		var listeningEndDate = notifyPeriod.endDate;
		return messageStartDate < listeningEndDate && messageEndDate > listeningStartDate;
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
		var nofityPeriod = _getNotifyPeriod();
		_checkIfScheduleHasUpdates().done(function (data) {
			if (!!data && !!data.HasUpdates) {
				_showNotice();
				currentListener && currentListener.callback && currentListener.callback(nofityPeriod);
			}
		});
	}

	function _getNotifyText() {
		return settings.notifyText;
	}

	function _getNotifyPeriod() {
		var noticeListeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours', -1).toDate();
		return {
			startDate: noticeListeningStartDate,
			endDate: moment(new Date(new Date().getTeleoptiTime())).add('days', 1).toDate()
		};
	}

	function _init(options) {
		ajax = new Teleopti.MyTimeWeb.Ajax();
		settings = $.extend({
			intervalTimeout: 1000 * 60 * 5
		}, options);

		var noticeListeningStartDate = moment(new Date(new Date().getTeleoptiTime())).add('hours', -1).toDate();

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
		GetNotifyText: _getNotifyText
	};
})(jQuery);