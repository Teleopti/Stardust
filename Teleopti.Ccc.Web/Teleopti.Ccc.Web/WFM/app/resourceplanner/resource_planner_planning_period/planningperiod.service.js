(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningPeriodService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var agentGroupBaseUrl = '../api/resourceplanner/agentgroup/:agentGroupId';
		var agentGroup = $resource(agentGroupBaseUrl, { agentGroupId: "@agentGroupId" },
		{
			nextPlanningPeriod: { method: 'POST', params: { agentGroupId: "@agentGroupId" }, url: agentGroupBaseUrl + '/nextplanningperiod' },
			getPlanningPeriods: { method: 'GET', params: { agentGroupId: "@agentGroupId" }, isArray: true, url: agentGroupBaseUrl + '/planningperiods' }
		});

		var service = {
			getPlanningPeriods: agentGroup.getPlanningPeriods,
			nextPlanningPeriod: agentGroup.nextPlanningPeriod
		};

		return service;
	}
})();
