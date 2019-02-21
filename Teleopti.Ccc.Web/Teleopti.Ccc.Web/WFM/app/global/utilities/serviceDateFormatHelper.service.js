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
			var localeSafeMoment = moment(date).locale('en');
			return localeSafeMoment.format(format);
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