(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('miscService', miscService);

	function miscService() {

		this.isIE = false || !!document.documentMode;

		this.getDateFromServer = function(date) {
			var dateToBefixed = new Date(date);
			if (!this.isIE) dateToBefixed.setTime(dateToBefixed.getTime() + dateToBefixed.getTimezoneOffset() * 60 * 1000);
			return dateToBefixed;
		};

		this.sendDateToServer = function(date) {
			return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0));
		};
	}
})();