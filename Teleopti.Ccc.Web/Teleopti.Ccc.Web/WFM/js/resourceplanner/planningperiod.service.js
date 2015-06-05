(function () {
	'use strict';
	angular.module('restPlanningPeriodService', ['ngResource'])
		.service('PlanningPeriodSvrc', [
		'$resource', function ($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/availableplanningperiod/', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
		}
		]).service('PlanningPeriodNewSvrc', [
		'$resource', function ($resource) {
			this.suggestions = $resource('../api/resourceplanner/planningperiod/nextsuggestions/', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.planningperiod = $resource('../api/resourceplanner/nextplanningperiod/', {}, {
				update: { method: 'POST', params: {}, isArray: false }
			});
		}
		]);
})();