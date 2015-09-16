define([
				'jquery',
				'messagebroker',
				'helpers'
], function (
			$,
			messagebroker,
			helpers
	) {
	
	var groupScheduleSubscription = null;

	var unsubscribeGroupSchedule = function () {
		if (!groupScheduleSubscription)
			return;
		messagebroker.unsubscribe(groupScheduleSubscription);
		groupScheduleSubscription = null;
	};

	var loadGroupSchedules = function (buid, date, groupId, callback) {
		$.ajax({
			url: 'GroupSchedule/Get',
			headers: { 'X-Business-Unit-Filter': buid },
			cache: false,
			dataType: 'json',
			data: {
				groupId: groupId,
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

	var currentThrottleTimeout;

	return {
		subscribeGroupSchedule: function (buId, groupId, date, peopleCheckCallback, callback) {
			unsubscribeGroupSchedule();
			messagebroker.started.done(function () {
				loadGroupSchedules(buId, date, groupId, callback);
				groupScheduleSubscription = messagebroker.subscribe({
					businessUnitId: buId,
					domainType: 'IPersonScheduleDayReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							if (peopleCheckCallback(notification.DomainReferenceId)) {
								if (!currentThrottleTimeout) {
									currentThrottleTimeout = setTimeout(function () {
										clearTimeout(currentThrottleTimeout);
										currentThrottleTimeout = undefined;
										loadGroupSchedules(buId, date, groupId, callback);
									}, 500);
								}
							}
						}
					}
				});
			});
		},
		unsubscribeGroupSchedule: unsubscribeGroupSchedule,
	};

});
