(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('agentGroupService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var agentGroup = $resource('../api/ResourcePlanner/AgentGroup');
		var agentGroupById = $resource('../api/ResourcePlanner/AgentGroup/:id', {id: "@id"});
		var filterResult = $resource('../api/filtersagentgroup', {searchString: '@searchString', maxHits: 100});

		var service = {
			getAgentGroups: agentGroup,
			saveAgentGroup: agentGroup,
			getAgentGroupbyId: agentGroupById,
			getFilterData: filterResult
		};
		return service;
	}
})();
