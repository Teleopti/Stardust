Teleopti.MyTimeWeb.PollScheduleUpdates = (function () {
	var interval =  5 * 60 * 1000; // hardcode to 5 min
	var listeners = [];
	var notifyOptions = null;
	var notificerDisplayTime = 0;

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
		//ajax.Ajax({
		//	url: 'ASM/CheckIfScheduleHasUpdates',
		//	success: function (hasUpdates) {
		//		deferred.resolve(hasUpdates);
		//	},
		//	error: function (jqXHR, textStatus, errorThrown) {
		//		deferred.reject(false);
		//	}
		//});
		deferred.resolve({
			HasUpdates: true,
			StartDate: '2017-11-10',
			EndDate: '2017-11-11'
		});
		return deferred.promise();
	}

	function _showNotice(period) {
		var startDate = _convertMbDateTimeToJsDate(period.startDate);
		var endDate = _convertMbDateTimeToJsDate(period.endDate);

		var isValid = _validSchedulePeriod(startDate, endDate);
		if (isValid) {
			var changedDateRange = new moment(startDate).format('L');
			if (period.startDate !== period.endDate) {
				changedDateRange = changedDateRange + ' - ' + new moment(endDate).format('L');
			}
			var notifyText = notifyOptions.notifyText.format(changedDateRange);
			if (!notificerDisplayTime) {
				Teleopti.MyTimeWeb.AlertActivity.GetNotificationDisplayTime(function (displayTime) {
					notifyOptions.timeout = notificerDisplayTime * 1000;
					Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, notifyText);
				});
				return;
			}

			Teleopti.MyTimeWeb.Notifier.Notify(notifyOptions, notifyText);
		}
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

	function _convertMbDateTimeToJsDate(mbDateTime) {
		var splitDatetime = mbDateTime.split('T');
		var splitDate = splitDatetime[0].split('-');
		return new Date(splitDate[0], splitDate[1] - 1, splitDate[2]);
	}

	function _init(options) {
		notifyOptions = options;

		setInterval(function () {
			listeners.forEach(function (listener) {
				var curPeriod = $.isFunction(listener.period) ? listener.period() : listener.period;
				_checkIfScheduleHasUpdates(curPeriod).done(function (data) {
					if (data.HasUpdates) {
						_showNotice({ startDate: data.StartDate, endDate: data.EndDate });
						listener.callback();
					}
				});
			});
		}, interval);
	}

	return {
		AddListener: _addListener,
		Init: _init
	};
})(jQuery);