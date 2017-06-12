(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('agentGroupService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var agentGroup = $resource('../api/resourceplanner/planninggroup/:id', { id: "@id" });
		var filterResult = $resource('../api/filtersplanninggroup', {searchString: '@searchString', maxHits: 100});

		var service = {
			getAgentGroups: agentGroup.query,
			saveAgentGroup: agentGroup.save,
			getAgentGroupById: agentGroup.get,
			getFilterData: filterResult.query,
			removeAgentGroup: agentGroup.remove
		};

		return service;
	}
})();
