(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('PlanGroupSettingService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {
		var setting = $resource('../api/resourceplanner/plangroupsetting/:id', { id: '@id' });
		var planningGroupDo = $resource('../api/resourceplanner/planninggroup/:planningGroupId/plangroupsetting', { planningGroupId: '@planningGroupId' });
		var filterResult = $resource('../api/filters', { searchString: '@searchString', maxHits: 100 });

		var service = {
			getSettingsByPlanningGroupId: planningGroupDo.query,
			removeSetting: setting.remove,
			getSetting: setting.get,
			saveSetting: setting.save,
			getFilterData: filterResult.query
		};

		return service;
	}
})();
