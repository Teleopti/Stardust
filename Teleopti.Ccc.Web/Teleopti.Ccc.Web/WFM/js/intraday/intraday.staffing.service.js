(function () {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayStaffingService', [
			'$resource', function ($resource) {
				this.resourceCalculate = $resource('../resourceCalculate', {  }, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});
			}
		]);
})();
