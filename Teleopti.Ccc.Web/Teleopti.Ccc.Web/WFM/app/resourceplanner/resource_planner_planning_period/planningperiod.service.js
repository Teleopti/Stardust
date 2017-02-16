(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningPeriodService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var planningPeriodByAgentGroupId = $resource('../api/resourceplanner/planningperiodforagentgroup/:agentGroupId', {
			agentGroupId: "@agentGroupId"
		});

		var nextplanningperiod = $resource('../api/resourceplanner/nextplanningperiod/:agentGroupId', {
			agentGroupId: "@agentGroupId"
		});

		var service = {
			getPlanningPeriodByAgentGroupId: planningPeriodByAgentGroupId,
			getNextPlanningPeriod: nextplanningperiod
		};

		return service;
	}
})();
