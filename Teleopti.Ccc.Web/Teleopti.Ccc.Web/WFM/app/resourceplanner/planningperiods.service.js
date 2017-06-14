(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningPeriodService', factory);

	factory.$inject = ['$resource', '$http'];

	function factory($resource, $http) {

		var planningPeriodBaseUrl = '../api/resourceplanner/planningperiod/:id';
		var planningPeriod = $resource(planningPeriodBaseUrl, { id: "@id" },
		{
			lastJobStatus: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/status' },
			lastIntradayOptimizationJobStatus: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/intradaystatus' },
			lastJobResult: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/result' },
			getSuggestions: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/suggestions', isArray: true },//old one?
			publishPeriod: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/publish' },
			changeRange: { method: 'PUT', params: { id: "@id" } },
			schedule: { method: 'POST', params: { id: "@id", runAsynchronously: function (d) { return d.runAsynchronously } }, url: planningPeriodBaseUrl + '/schedule' },
			optimize: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/optimize' },
			clear: { method: 'DELETE', params: { id: "@id" }, url: planningPeriodBaseUrl + '/schedule' }
		});

		var getPlanningPeriodSuggestions = $resource('../api/resourceplanner/planningperiod/suggestions/:planningGroupId');

		var planningGroupBaseUrl = '../api/resourceplanner/planninggroup/:planningGroupId';
		var planningGroup = $resource(planningGroupBaseUrl, { planningGroupId: "@planningGroupId" },
		{
			nextPlanningPeriod: { method: 'POST', params: { planningGroupId: "@planningGroupId" }, url: planningGroupBaseUrl + '/nextplanningperiod' },
			getPlanningPeriods: { method: 'GET', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: planningGroupBaseUrl + '/planningperiods' },
			deleteLastPlanningPeriod: { method: 'DELETE', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: planningGroupBaseUrl + '/lastperiod' },
			changeEndDateForLastPlanningPeriod: { method: 'PUT', params: { planningGroupId: "@planningGroupId", startDate:"@startDate", endDate:"@endDate" }, isArray: true, url: planningGroupBaseUrl + '/lastperiod' }
		});

		var deprecatedPlanningperiod = $resource('../api/resourceplanner/nextplanningperiod', {}, {
			update: { method: 'POST', params: {}, isArray: false }
		});

		var service = {
			// PlanningPeriod + PlanningGroup
			getPlanningGroupById: planningGroup.get,
			getPlanningPeriodsForPlanningGroup: planningGroup.getPlanningPeriods,
			nextPlanningPeriod: planningGroup.nextPlanningPeriod,
			deleteLastPlanningPeriod: planningGroup.deleteLastPlanningPeriod,
			changeEndDateForLastPlanningPeriod: planningGroup.changeEndDateForLastPlanningPeriod,
			// PlanningPeriods
			getPlanningPeriod: planningPeriod.get,
			getPlanningPeriods: planningPeriod.query,
			getPlanningPeriodSuggestions: getPlanningPeriodSuggestions.query,

			lastJobStatus: planningPeriod.lastJobStatus,
			lastIntradayOptimizationJobStatus: planningPeriod.lastIntradayOptimizationJobStatus,
			lastJobResult: planningPeriod.lastJobResult,
			changeRange: planningPeriod.changeRange,
			getSuggestions: planningPeriod.getSuggestions,
			publishPeriod: planningPeriod.publishPeriod,
			// Scheduling
			launchScheduling: planningPeriod.schedule,
			launchOptimization: planningPeriod.optimize,
			clearSchedules : planningPeriod.clear,
			// TODO: deprecate once schedule/optimization on stardust
			keepAlive: function() {
				return $http({ url: '../api/resourceplanner/keepalive', method: 'POST' });
			},
			// TODO: deprecate once we always use planning group
			nextPlanningPeriodDeprecated: deprecatedPlanningperiod.update
		};

		return service;
	}
})();
