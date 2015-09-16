define([
				'jquery',
				'messagebroker',
				'helpers',
], function (
			$,
			messagebroker,
			helpers
	) {

	var personScheduleSubscription = null;

	var unsubscribePersonSchedule = function () {
		if (!personScheduleSubscription)
			return;
		messagebroker.unsubscribe(personScheduleSubscription);
		personScheduleSubscription = null;
	};

	var loadPersonSchedules = function (buid, date, personId, callback) {
		$.ajax({
			url: 'PersonSchedule/Get',
			headers: { 'X-Business-Unit-Filter': buid },
			cache: false,
			dataType: 'json',
			data: {
				personId: personId,
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
		subscribePersonSchedule: function (buId, personId, date, callback) {
			unsubscribePersonSchedule();
			messagebroker.started.done(function () {
				loadPersonSchedules(buId, date, personId, callback);

				personScheduleSubscription = messagebroker.subscribe({
					businessUnitId: buId,
					domainReferenceType: 'Person',
					domainReferenceId: personId,
					domainType: 'IPersonScheduleDayReadModel',
					callback: function (notification) {
						if (isMatchingDates(date, notification.StartDate, notification.EndDate)) {
							loadPersonSchedules(buId, date, personId, callback);
						}
					}
				});

			});
		},
		
		unsubscribePersonSchedule: unsubscribePersonSchedule
	};

});
