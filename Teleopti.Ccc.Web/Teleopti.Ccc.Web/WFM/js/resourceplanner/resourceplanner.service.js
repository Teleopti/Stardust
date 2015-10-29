(function () {
	'use strict';
	angular.module('restPlanningPeriodService', ['ngResource'])
		.service('ResourcePlannerSvrc', [
		'$resource', function ($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.getPlanningPeriodsForRange = $resource('../api/resourceplanner/planningperiodsforrange?startDate=:startDate&endDate=:endDate', {},
				{
					startDate: "@startDate",
					endDate: "@endDate"
				},{
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.getDayoffRules = $resource('../api/resourceplanner/dayoffrules/default', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.saveDayoffRules = $resource('../api/resourceplanner/dayoffrules', {}, {
				update: { method: 'POST', params: {}, isArray: false }
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
