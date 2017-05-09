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
			getAgentCount: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/countagents' },
			clear: { method: 'DELETE', params: { id: "@id" }, url: planningPeriodBaseUrl + '/schedule' }
		});

		var getPlanningPeriodSuggestions = $resource('../api/resourceplanner/planningperiod/suggestions/:agentGroupId');

		var agentGroupBaseUrl = '../api/resourceplanner/agentgroup/:agentGroupId';
		var agentGroup = $resource(agentGroupBaseUrl, { agentGroupId: "@agentGroupId" },
		{
			nextPlanningPeriod: { method: 'POST', params: { agentGroupId: "@agentGroupId" }, url: agentGroupBaseUrl + '/nextplanningperiod' },
			getPlanningPeriods: { method: 'GET', params: { agentGroupId: "@agentGroupId" }, isArray: true, url: agentGroupBaseUrl + '/planningperiods' },
			deleteLastPlanningPeriod: { method: 'DELETE', params: { agentGroupId: "@agentGroupId" }, isArray: true, url: agentGroupBaseUrl + '/lastperiod' },
			changeEndDateForLastPlanningPeriod: { method: 'PUT', params: { agentGroupId: "@agentGroupId", startDate:"@startDate", endDate:"@endDate" }, isArray: true, url: agentGroupBaseUrl + '/lastperiod' }
		});

		var deprecatedPlanningperiod = $resource('../api/resourceplanner/nextplanningperiod', {}, {
			update: { method: 'POST', params: {}, isArray: false }
		});

		var service = {
			// PlanningPeriod + AgentGroup
			getAgentGroupById: agentGroup.get,
			getPlanningPeriodsForAgentGroup: agentGroup.getPlanningPeriods,
			nextPlanningPeriod: agentGroup.nextPlanningPeriod,
			deleteLastPlanningPeriod: agentGroup.deleteLastPlanningPeriod,
			changeEndDateForLastPlanningPeriod: agentGroup.changeEndDateForLastPlanningPeriod,
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
			getNumberOfAgents: planningPeriod.getAgentCount,
			// Scheduling
			launchScheduling: planningPeriod.schedule,
			launchOptimization: planningPeriod.optimize,
			clearSchedules : planningPeriod.clear,
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
