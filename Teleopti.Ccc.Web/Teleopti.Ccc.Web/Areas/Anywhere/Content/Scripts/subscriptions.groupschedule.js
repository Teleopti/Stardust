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
			helpers,
			errorview
	) {

	var startPromise;

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
		startPromise.done(function () {
			incomingGroupSchedule = null;
			messagebroker.unsubscribe(groupScheduleSubscription);
			groupScheduleSubscription = null;
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
		start: function () {
			startPromise = messagebroker.start();
			return startPromise;
		},
		
		subscribeGroupSchedule: function (groupId, date, peopleCheckCallback, callback) {
			unsubscribeGroupSchedule();
			incomingGroupSchedule = callback;
			startPromise.done(function () {
				groupScheduleHub.server.subscribeGroupSchedule(groupId, date);

				groupScheduleSubscription = messagebroker.subscribe({
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
