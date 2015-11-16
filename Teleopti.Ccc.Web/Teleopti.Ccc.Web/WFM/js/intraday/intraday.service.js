(function() {
	'use strict';
	var intradayService = angular.module('wfm.intraday');
	intradayService.service('IntradayService', [
		'$resource', function ($resource) {

			this.skillList = $resource('../api/intraday/skillstatus', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});

			this.formatDateTime = function (time) {
				if (time === null || time === undefined || time === '') return '--:--:--';
				var momentTime = moment(time);
				if (momentTime.format("YYYY") > moment("1970").format("YYYY")) {
					return momentTime.format('HH:mm:ss');
				} else {
					return '--:--:--';
				}
			};
		}
	]);
})();