(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningGroupService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var planningGroup = $resource('../api/resourceplanner/planninggroup/:id', { id: "@id" });
		var filterResult = $resource('../api/filtersplanninggroup', {searchString: '@searchString', maxHits: 100});

		var service = {
			getAgentGroups: planningGroup.query,
			saveAgentGroup: planningGroup.save,
			getAgentGroupById: planningGroup.get,
			getFilterData: filterResult.query,
			removeAgentGroup: planningGroup.remove
		};

		return service;
	}
})();
