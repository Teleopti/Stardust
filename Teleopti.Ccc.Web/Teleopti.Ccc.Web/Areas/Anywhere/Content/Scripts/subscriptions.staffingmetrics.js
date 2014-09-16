define([
				'jquery',
				'messagebroker',
				'signalrhubs',
				'helpers',
				'errorview'
], function (
			$,
			messagebroker,
			signalRHubs,
			helpers
	) {

	var startPromise;

	var dailyStaffingMetricsSubscription = null;

	var unsubscribeDailyStaffingMetrics = function () {
		if (!dailyStaffingMetricsSubscription)
			return;
		startPromise.done(function () {
			messagebroker.unsubscribe(dailyStaffingMetricsSubscription);
			dailyStaffingMetricsSubscription = null;
		});
	};

	var loadDailyStaffingMetrics = function (buid, date, skillId, callback) {
		$.ajax({
			url: 'StaffingMetrics/DailyStaffingMetrics',
			headers: { 'X-Business-Unit-Filter': buid },
			cache: false,
			dataType: 'json',
			data: {
				skillId: skillId,
				date: date
			},
			success: function (data, textStatus, jqXHR) {
				callback(data);
			}
		});
	};

	var isMatchingDates = function (date, notificationStartDate, notificationEndDate) {
		var momentDate = moment(date);
		var startDate = helpers.Date.ToMoment(notificationStartDate).startOf('day');
		var endDate = helpers.Date.ToMoment(notificationEndDate).startOf('day');

		if (momentDate.diff(startDate) >= 0 && momentDate.diff(endDate) <= 0) return true;

		return false;
	};

	return {
		start: function () {
			startPromise = messagebroker.start();
			return startPromise;
		},

		subscribeDailyStaffingMetrics: function (buid, date, skillId, callback) {
			unsubscribeDailyStaffingMetrics();
			startPromise.done(function () {
				loadDailyStaffingMetrics(buid, date, skillId, callback);

				dailyStaffingMetricsSubscription = messagebroker.subscribe({
					businessUnitId: buid,
					domainType: 'IScheduledResourcesReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							loadDailyStaffingMetrics(date, skillId, callback);
						}
					}
				});
			});
		},

		unsubscribeDailyStaffingMetrics: unsubscribeDailyStaffingMetrics
	};

});
