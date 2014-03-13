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

	var personScheduleHub = $.connection.personScheduleHub;
	personScheduleHub.client.exceptionHandler = errorview.display;
	var personScheduleSubscription = null;
	var incomingPersonSchedule = null;
	personScheduleHub.client.incomingPersonSchedule = function (data) {
		if (incomingPersonSchedule != null)
			logException(function () { incomingPersonSchedule(data); });
	};

	var dailyStaffingMetricsSubscription = null;

	var start = function () {
		startPromise = messagebroker.start();
		return startPromise;
	};

	var unsubscribePersonSchedule = function () {
		if (!personScheduleSubscription)
			return;
		startPromise.done(function () {
			incomingPersonSchedule = null;
			messagebroker.unsubscribe(personScheduleSubscription);
			personScheduleSubscription = null;
		});
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

	var unsubscribeDailyStaffingMetrics = function () {
		if (!dailyStaffingMetricsSubscription)
			return;
		startPromise.done(function () {
			messagebroker.unsubscribe(dailyStaffingMetricsSubscription);
			dailyStaffingMetricsSubscription = null;
		});
	};

	var loadDailyStaffingMetrics = function (date, skillId, callback) {
		$.ajax({
			url: 'StaffingMetrics/DailyStaffingMetrics',
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
		start: start,

		subscribeDailyStaffingMetrics: function (date, skillId, callback) {
			unsubscribeDailyStaffingMetrics();
			startPromise.done(function () {
				loadDailyStaffingMetrics(date, skillId, callback);

				dailyStaffingMetricsSubscription = messagebroker.subscribe({
					domainType: 'IScheduledResourcesReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							loadDailyStaffingMetrics(date, skillId, callback);
						}
					}
				});
			});
		},

		subscribeGroupSchedule: function (groupId, date, callback) {
			unsubscribeGroupSchedule();
			incomingGroupSchedule = callback;
			startPromise.done(function () {
				groupScheduleHub.server.subscribeGroupSchedule(groupId, date);

				groupScheduleSubscription = messagebroker.subscribe({
					domainType: 'IPersonScheduleDayReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							groupScheduleHub.server.subscribeGroupSchedule(groupId, date);
						}
					}
				});
			});
		},

		subscribePersonSchedule: function (personId, date, callback) {
			unsubscribePersonSchedule();
			incomingPersonSchedule = callback;
			startPromise.done(function () {

				personScheduleHub.server.personSchedule(personId, date);

				personScheduleSubscription = messagebroker.subscribe({
					domainReferenceType: 'Person',
					domainReferenceId: personId,
					domainType: 'IPersonScheduleDayReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							personScheduleHub.server.personSchedule(personId, date);
						}
					}
				});

			});
		},
		
		unsubscribePersonSchedule: unsubscribePersonSchedule,
		unsubscribeGroupSchedule: unsubscribeGroupSchedule,
		unsubscribeDailyStaffingMetrics: unsubscribeDailyStaffingMetrics
	};

});
