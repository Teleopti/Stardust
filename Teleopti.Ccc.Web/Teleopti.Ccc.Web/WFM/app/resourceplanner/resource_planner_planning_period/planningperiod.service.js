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
				lastJobResult: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/result' },
				publishPeriod: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/publish' },
				schedule: { method: 'POST', params: { id: "@id" }, url: planningPeriodBaseUrl + '/schedule' },
				intraOptimize: { method: 'POST', params: { id: '@id' }, url: planningPeriodBaseUrl + '/optimizeintraday' },
				getValidation: { method: 'GET', params: { id: "@id" }, url: planningPeriodBaseUrl + '/validation' },
				clear: { method: 'DELETE', params: { id: "@id" }, url: planningPeriodBaseUrl + '/schedule' },

			});

		var planningGroupBaseUrl = '../api/resourceplanner/planninggroup/:planningGroupId';
		var planningGroup = $resource(planningGroupBaseUrl, { planningGroupId: "@planningGroupId" },
			{
				nextPlanningPeriod: { method: 'POST', params: { planningGroupId: "@planningGroupId" }, url: planningGroupBaseUrl + '/nextplanningperiod' },
				getPlanningPeriods: { method: 'GET', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: planningGroupBaseUrl + '/planningperiods' },
				deleteLastPlanningPeriod: { method: 'DELETE', params: { planningGroupId: "@planningGroupId" }, isArray: true, url: planningGroupBaseUrl + '/lastperiod' },
				changeEndDateForLastPlanningPeriod: { method: 'PUT', params: { planningGroupId: "@planningGroupId", startDate: "@startDate", schedulePeriodType: "@schedulePeriodType", lengthOfThePeriodType: "@lengthOfThePeriodType", endDate: "@endDate" }, isArray: true, url: planningGroupBaseUrl + '/lastperiod' },
				firstPlanningPeriod: { method: 'POST', params: { planningGroupId: "@planningGroupId", startDate: "@startDate", schedulePeriodType: "@schedulePeriodType", lengthOfThePeriodType:"@lengthOfThePeriodType" }, url: planningGroupBaseUrl + '/firstplanningperiod' }
			});

		return {
			// PlanningPeriod.Select
			getPlanningGroupById: planningGroup.get,
			getPlanningPeriodsForPlanningGroup: planningGroup.getPlanningPeriods,
			nextPlanningPeriod: planningGroup.nextPlanningPeriod,
			deleteLastPlanningPeriod: planningGroup.deleteLastPlanningPeriod,
			changeEndDateForLastPlanningPeriod: planningGroup.changeEndDateForLastPlanningPeriod,
			getPlanningPeriod: planningPeriod.get,
			getPlanningPeriodSuggestions: getPlanningPeriodSuggestions.query,
			firstPlanningPeriod: planningGroup.firstPlanningPeriod,
			// PlanningPeriod.Overview
			getValidation: planningPeriod.getValidation,
			lastJobResult: planningPeriod.lastJobResult,
			lastJobStatus: planningPeriod.lastJobStatus,
			// Scheduling
			launchScheduling: planningPeriod.schedule,
			launchIntraOptimize: planningPeriod.intraOptimize,
			clearSchedules: planningPeriod.clear,
			publishPeriod: planningPeriod.publishPeriod
		};
	}
})();
