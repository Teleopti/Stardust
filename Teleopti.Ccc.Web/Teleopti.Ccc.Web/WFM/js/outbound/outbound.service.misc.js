(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('miscService', ['$locale', miscService]);

	function miscService($locale) {

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

		this.parseNumberString = function (s, integerOnly) {
			var gSize = $locale.NUMBER_FORMATS.PATTERNS[0].gSize;
			var pieces = ('' + s).split($locale.NUMBER_FORMATS.DECIMAL_SEP),
				whole = pieces[0],
				fraction = pieces[1] || '';

			if (pieces.length > 2) return false;
			if (integerOnly && fraction.length > 0) return false;

			var _whole = whole.replace(/\s+/g, '').split($locale.NUMBER_FORMATS.GROUP_SEP).join(' ');

			var testWhole = new RegExp('^[-+]?[0-9]{1,' + gSize + '}( ?[0-9]{' + gSize + '})*$');
			if (!testWhole.test(_whole)) return false;

			if (fraction.length > 0) {
				if (!/^[0-9]+$/.test(fraction)) return false;
			}
			return integerOnly?parseInt(_whole.replace(/\s+/g, '')):parseFloat(_whole.replace(/\s+/g, '') + '.' + fraction);
		}


	}
})();