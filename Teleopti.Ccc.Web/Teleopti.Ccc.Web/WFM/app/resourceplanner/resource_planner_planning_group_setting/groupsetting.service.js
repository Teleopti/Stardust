(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('PlanGroupSettingService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {
		var dayOffRule = $resource('../api/resourceplanner/dayoffrules/:id', { id: '@id' });
		var planningGroupDo = $resource('../api/resourceplanner/planninggroup/:planningGroupId/dayoffrules', { planningGroupId: '@planningGroupId' });
		var filterResult = $resource('../api/filters', { searchString: '@searchString', maxHits: 100 });

		var service = {
			getDayOffRules: dayOffRule.query,
			getDayOffRulesByPlanningGroupId: planningGroupDo.query,
			removeDayOffRule: dayOffRule.remove,
			getDayOffRule: dayOffRule.get,
			saveDayOffRule: dayOffRule.save,
			getFilterData: filterResult.query
		};

		return service;
	}
})();
