(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('planningGroupService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {

		var planningGroup = $resource('../api/resourceplanner/planninggroup/:id', { id: "@id" });
		var filterResult = $resource('../api/filtersplanninggroup', {searchString: '@searchString', maxHits: 100});

		return {
			getPlanningGroups: planningGroup.query,
			savePlanningGroup: planningGroup.save,
			getPlanningGroupById: planningGroup.get,
			getFilterData: filterResult.query,
			removePlanningGroup: planningGroup.remove
		};
	}
})();
