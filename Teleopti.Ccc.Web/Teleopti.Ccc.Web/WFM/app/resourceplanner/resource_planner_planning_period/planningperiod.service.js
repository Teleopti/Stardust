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
			getSuggestions: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/suggestions', isArray: true },
			publishPeriod: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/publish' },
			changeRange: { method: 'PUT', params: { id: "@id" } },
			schedule: { method: 'POST', params: { id: "@id", runAsynchronously: function (d) { return d.runAsynchronously } }, url: planningPeriodBaseUrl + '/schedule' },
			optimize: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/optimize' }
		});

		var agentGroupBaseUrl = '../api/resourceplanner/agentgroup/:agentGroupId';
		var agentGroup = $resource(agentGroupBaseUrl, { agentGroupId: "@agentGroupId" },
		{
			nextPlanningPeriod: { method: 'POST', params: { agentGroupId: "@agentGroupId" }, url: agentGroupBaseUrl + '/nextplanningperiod' },
			getPlanningPeriods: { method: 'GET', params: { agentGroupId: "@agentGroupId" }, isArray: true, url: agentGroupBaseUrl + '/planningperiods' }
		});

		var deprecatedPlanningperiod = $resource('../api/resourceplanner/nextplanningperiod', {}, {
			update: { method: 'POST', params: {}, isArray: false }
		});

		var service = {
			// PlanningPeriod + AgentGroup
			getPlanningPeriodsForAgentGroup: agentGroup.getPlanningPeriods,
			nextPlanningPeriod: agentGroup.nextPlanningPeriod,

			// PlanningPeriods
			getPlanningPeriod: planningPeriod.get,
			getPlanningPeriods: planningPeriod.query,
			lastJobStatus: planningPeriod.lastJobStatus,
			lastIntradayOptimizationJobStatus: planningPeriod.lastIntradayOptimizationJobStatus,
			lastJobResult: planningPeriod.lastJobResult,
			changeRange: planningPeriod.changeRange,
			getSuggestions: planningPeriod.getSuggestions,
			publishPeriod: planningPeriod.publishPeriod,
			// Scheduling
			launchScheduling: planningPeriod.schedule,
			launchOptimization: planningPeriod.optimize,
			// TODO: deprecate once schedule/optimization on stardust
			keepAlive: function() {
				return $http({ url: '../api/resourceplanner/keepalive', method: 'POST' });
			},
			// TODO: deprecate once we always use agent group
			nextPlanningPeriodDeprecated: deprecatedPlanningperiod.update
		};

		return service;
	}
})();
