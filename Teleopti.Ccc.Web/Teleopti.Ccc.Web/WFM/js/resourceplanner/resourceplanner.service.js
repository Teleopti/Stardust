(function () {
	'use strict';
	angular.module('restPlanningPeriodService', ['ngResource'])
		.service('ResourcePlannerSvrc', [
		'$resource', function ($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
		}
		]).service('PlanningPeriodNewSvrc', [
		'$resource', function ($resource) {
			this.planningperiod = $resource('../api/resourceplanner/nextplanningperiod', {}, {
				update: { method: 'POST', params: {}, isArray: false }
			});
		}
		]);
})();