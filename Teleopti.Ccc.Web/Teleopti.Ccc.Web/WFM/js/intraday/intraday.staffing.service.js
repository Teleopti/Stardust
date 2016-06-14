(function () {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayStaffingService', [
			'$resource', function ($resource) {
				this.resourceCalculate = $resource('../resourcecalculate', {date: '@date'}, {
					query: {
						method: 'GET',
						params: {date:name},
						isArray: false
					}
				});
			}
		]);
})();
