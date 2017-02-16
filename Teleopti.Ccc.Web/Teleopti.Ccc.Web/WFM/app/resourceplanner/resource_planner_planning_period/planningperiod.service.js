(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningPeriodService', factory);

	factory.$inject = ['$resource'];

	/* @ngInject */
	function factory($resource) {

		var planningPeriodByAgentGroupId = $resource('../api/resourceplanner/planningperiodforagentgroup/:agentGroupId', {
			agentGroupId: "@id"
		});

		var nextplanningperiod = $resource('../api/resourceplanner/nextplanningperiod/:agentGroupId', {agentGroupId: "@id"}, {
			update: {
				method: 'POST',
				params: {},
				isArray: false
			}
		});

		var service = {
			getPlanningPeriodByAgentGroupId: planningPeriodByAgentGroupId,
			getNextPlanningPeriod: nextplanningperiod
		};


		return service;
	}
})();
