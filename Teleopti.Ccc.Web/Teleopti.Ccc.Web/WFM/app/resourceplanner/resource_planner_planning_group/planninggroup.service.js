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
			getPlanGroups: planningGroup.query,
			savePlanGroup: planningGroup.save,
			getPlanGroupById: planningGroup.get,
			getFilterData: filterResult.query,
			removePlanGroup: planningGroup.remove
		};

		return service;
	}
})();
