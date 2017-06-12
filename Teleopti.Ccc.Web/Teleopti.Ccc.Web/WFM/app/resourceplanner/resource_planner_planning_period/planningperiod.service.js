(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningPeriodServiceNew', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var getPlanningPeriodSuggestions = $resource('../api/resourceplanner/planningperiod/suggestions/:planningGroupId');
		var planningPeriodBaseUrl = '../api/resourceplanner/planningperiod/:id';
		var planningPeriod = $resource(planningPeriodBaseUrl, { id: "@id" },
			{
				lastJobStatus: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/status' },
				lastIntradayOptimizationJobStatus: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/intradaystatus' },
				lastJobResult: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/result' },
				publishPeriod: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/publish' },
				schedule: { method: 'POST', params: { id: "@id", runAsynchronously: function (d) { return d.runAsynchronously } }, url: planningPeriodBaseUrl + '/schedule' },
				intraOptimize: { method: 'POST', params: { id: '@id', runAsynchronously: function (d) { return d.runAsynchronously } }, url: planningPeriodBaseUrl + '/optimizeintraday' },
				getValidation: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/validation' },
				clear: { method: 'DELETE', params: { id: "@id" }, url: planningPeriodBaseUrl + '/schedule' },

			});

		var agentGroupBaseUrl = '../api/resourceplanner/planninggroup/:planningGroupId';
		var agentGroup = $resource(agentGroupBaseUrl, { planningGroupId: "@planningGroupId" },
			{
				nextPlanningPeriod: { method: 'POST', params: { planningGroupId: "@planningGroupId" }, url: agentGroupBaseUrl + '/nextplanningperiod' },
				getPlanningPeriods: { method: 'GET', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: agentGroupBaseUrl + '/planningperiods' },
				deleteLastPlanningPeriod: { method: 'DELETE', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: agentGroupBaseUrl + '/lastperiod' },
				changeEndDateForLastPlanningPeriod: { method: 'PUT', params: { planningGroupId: "@planningGroupId", startDate: "@startDate", endDate: "@endDate" }, isArray: true, url: agentGroupBaseUrl + '/lastperiod' },
				firstPlanningPeriod: { method: 'POST', params: { planningGroupId: "@planningGroupId", startDate: "@startDate", endDate: "@endDate" }, url: agentGroupBaseUrl + '/firstplanningperiod' }
			});

		var service = {
			// PlanningPeriod.Select
			getAgentGroupById: agentGroup.get,
			getPlanningPeriodsForAgentGroup: agentGroup.getPlanningPeriods,
			nextPlanningPeriod: agentGroup.nextPlanningPeriod,
			deleteLastPlanningPeriod: agentGroup.deleteLastPlanningPeriod,
			changeEndDateForLastPlanningPeriod: agentGroup.changeEndDateForLastPlanningPeriod,
			getPlanningPeriod: planningPeriod.get,
			getPlanningPeriodSuggestions: getPlanningPeriodSuggestions.query,
			firstPlanningPeriod: agentGroup.firstPlanningPeriod,
			// PlanningPeriod.Overview
			getValidation: planningPeriod.getValidation,
			lastJobResult: planningPeriod.lastJobResult,
			lastJobStatus: planningPeriod.lastJobStatus,
			lastIntradayOptimizationJobStatus: planningPeriod.lastIntradayOptimizationJobStatus,
			// Scheduling
			launchScheduling: planningPeriod.schedule,
			launchIntraOptimize: planningPeriod.intraOptimize,
			clearSchedules: planningPeriod.clear,
			publishPeriod: planningPeriod.publishPeriod
		};

		return service;
	}
})();
