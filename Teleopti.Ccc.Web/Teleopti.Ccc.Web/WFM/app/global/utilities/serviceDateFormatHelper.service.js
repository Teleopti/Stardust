(function () {
	'use strict';

	angular
		.module('wfm.utilities')
		.service('serviceDateFormatHelper', serviceDateFormatHelper);

	function serviceDateFormatHelper() {
		var serviceDateFormat = {
			dateTime: "YYYY-MM-DD HH:mm",
			dateOnly: "YYYY-MM-DD",
			timeOnly: "HH:mm"
		};
		var self = this;

		this.getDateByFormat = function (date, format) {
			var dateMoment = date;
			if (!(date instanceof moment)) {
				dateMoment = moment(date);
			}

			if (moment.locale() != 'en') {
				dateMoment.locale('en');
			}

			return dateMoment.format(format);
		}

		this.getDateOnly = function (date) {
			return self.getDateByFormat(date, serviceDateFormat.dateOnly);
		}

		this.getTimeOnly = function (dateTime) {
			return self.getDateByFormat(dateTime, serviceDateFormat.timeOnly);
		}

		this.getDateTime = function (date) {
			return self.getDateByFormat(date, serviceDateFormat.dateTime);
		}
	}
})();