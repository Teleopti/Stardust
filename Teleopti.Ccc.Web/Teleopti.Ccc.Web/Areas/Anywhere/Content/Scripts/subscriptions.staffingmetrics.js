define([
				'jquery',
				'messagebroker',
				'helpers'
], function (
			$,
			messagebroker,
			helpers
	) {

	var dailyStaffingMetricsSubscription = null;

	var unsubscribeDailyStaffingMetrics = function () {
		if (!dailyStaffingMetricsSubscription)
			return;
		messagebroker.unsubscribe(dailyStaffingMetricsSubscription);
		dailyStaffingMetricsSubscription = null;
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
		subscribeDailyStaffingMetrics: function (buid, date, skillId, callback) {
			unsubscribeDailyStaffingMetrics();
			messagebroker.started.done(function () {
				loadDailyStaffingMetrics(buid, date, skillId, callback);

				dailyStaffingMetricsSubscription = messagebroker.subscribe({
					businessUnitId: buid,
					domainType: 'IScheduledResourcesReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							loadDailyStaffingMetrics(buid,date, skillId, callback);
						}
					}
				});
			});
		},

		unsubscribeDailyStaffingMetrics: unsubscribeDailyStaffingMetrics
	};

});
