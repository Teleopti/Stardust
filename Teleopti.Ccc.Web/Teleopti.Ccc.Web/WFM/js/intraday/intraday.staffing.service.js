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
				this.TriggerResourceCalculate = $resource('../TriggerResourceCalculate', {}, {
					query: {
						method: 'GET',
						params: {},
						isArray: false
					}
				});
			}
		]);
})();
