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
	
	var personScheduleHub = $.connection.personScheduleHub;
	personScheduleHub.client.exceptionHandler = errorview.display;
	var personScheduleSubscription = null;
	var incomingPersonSchedule = null;
	personScheduleHub.client.incomingPersonSchedule = function (data) {
		if (incomingPersonSchedule != null)
			logException(function () { incomingPersonSchedule(data); });
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

		subscribePersonSchedule: function (buId, personId, date, callback) {
			unsubscribePersonSchedule();
			incomingPersonSchedule = callback;
			startPromise.done(function () {
				personScheduleHub.connection.qs = { "BusinessUnitId": buId };
				personScheduleHub.server.personSchedule(personId, date);

				personScheduleSubscription = messagebroker.subscribe({
					businessUnitId: buId,
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
		
		unsubscribePersonSchedule: unsubscribePersonSchedule
	};

});
