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

		this.getDateOnly = function (date) {
			var localeSafeMoment = moment(date).locale('en');
			return localeSafeMoment.format(serviceDateFormat.dateOnly);
		}

		this.getTimeOnly = function (date) {
			var localeSafeMoment = moment(date).locale('en');
			return localeSafeMoment.format(serviceDateFormat.timeOnly);
		}

		this.getDateTime = function (date) {
			var localeSafeMoment = moment(date).locale('en');
			return localeSafeMoment.format(serviceDateFormat.dateTime);
		}
	}
})();