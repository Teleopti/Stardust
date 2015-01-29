define([
				'jquery',
				'messagebroker',
				'signalrhubs.started',
				'helpers',
				'errorview'
], function (
			$,
			messagebroker,
			started,
			helpers,
			errorview
	) {

	// why is signalr eating my exceptions?
	var logException = function (func) {
		try {
			func();
		} catch (e) {
			errorview.display(e);
			throw e;
		}
	};
	
	var groupScheduleHub = $.connection.groupScheduleHub;
	groupScheduleHub.client.exceptionHandler = errorview.display;
	var incomingGroupSchedule = null;
	var groupScheduleSubscription = null;
	groupScheduleHub.client.incomingGroupSchedule = function (data) {
		if (incomingGroupSchedule != null)
			logException(function () { incomingGroupSchedule(data); });
	};
	
	var unsubscribeGroupSchedule = function () {
		if (!groupScheduleSubscription)
			return;
		incomingGroupSchedule = null;
		messagebroker.unsubscribe(groupScheduleSubscription);
		groupScheduleSubscription = null;
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
			incomingGroupSchedule = callback;
			started.done(function () {
				groupScheduleHub.connection.qs = { "BusinessUnitId": buId };
				groupScheduleHub.server.subscribeGroupSchedule(groupId, date);

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
										groupScheduleHub.server.subscribeGroupSchedule(groupId, date);
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
