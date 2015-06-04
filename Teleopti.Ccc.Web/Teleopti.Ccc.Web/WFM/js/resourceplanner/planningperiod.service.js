(function () {
	'use strict';
	angular.module('restPlanningPeriodService', ['ngResource']).service('PlanningPeriodSvrc', [
		'$resource', function ($resource) {
			//planning period
			this.getPlanningPeriod = $resource('..api/resourceplanner/availableplanningperiod/', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
		}
	]);
})();