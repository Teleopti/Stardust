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
				console.log(time);
				var momentTime = moment.utc(time);
				if (momentTime.format("YYYY") > moment("1970").format("YYYY")) {
					return momentTime.format('HH:mm:ss');
				} else {
					return '--:--:--';
				}
			};
		}
	]);
})();