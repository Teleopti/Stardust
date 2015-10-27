(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('miscService', miscService);

	function miscService() {

		this.isIE = false || !!document.documentMode;

		this.getDateFromServer = function (date) {
			var dateToBefixed = new Date(date);			
			dateToBefixed.setTime(dateToBefixed.getTime() + dateToBefixed.getTimezoneOffset() * 60 * 1000);			
			return dateToBefixed;
		};

		this.sendDateToServer = function(date) {
			return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0));
		};

		this.isLastDayOfMonth = function(momentDate) {
			var next = momentDate.clone().add(1, 'day');
			return next.format('M') !== momentDate.format('M');
		};

		this.getAllWeekendsInPeriod = function (period) {			
			var periodStart = moment(period.PeriodStart),
				periodEnd = moment(period.PeriodEnd);

			var weekends = [];
			if (periodStart.day() == 0) {				
				weekends.push({
					WeekendStart: periodStart.clone(),
					WeekendEnd: periodStart.clone()
				});
			}

			var iterator = periodStart.clone().add(6 - periodStart.day(), 'day');
			while (iterator <= periodEnd) {
				var weekendStart = iterator.clone(),
					weekendEnd = iterator.clone().add(1, 'day');

				weekends.push({
					WeekendStart: weekendStart,
					WeekendEnd: (weekendEnd > periodEnd) ? periodEnd : weekendEnd
				});

				iterator.add(7, 'day');
			}
			return weekends.map(function(w) {
				return {
					WeekendStart: w.WeekendStart.toDate(),
					WeekendEnd: w.WeekendEnd.toDate()
				}
			});
		};

	}
})();