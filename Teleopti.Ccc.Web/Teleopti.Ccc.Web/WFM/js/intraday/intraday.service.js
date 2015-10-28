(function() {
	'use strict';

	var intradayService = angular.module('wfm.intraday');
	intradayService.service('IntradayService', [
		'$resource', function ($resource) {

			this.skillList = $resource('../api/intraday/skillstatus', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
		}
	]);
})();