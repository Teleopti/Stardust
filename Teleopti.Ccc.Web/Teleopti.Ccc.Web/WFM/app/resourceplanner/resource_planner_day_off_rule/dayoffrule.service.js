(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('dayOffRuleService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {
		var dayOffRule = $resource('../api/resourceplanner/dayoffrules/:id', { id: '@id' });
		var filterResult = $resource('../api/filters', { searchString: '@searchString', maxHits: 100 });

		var service = {
			getDayOffRules: dayOffRule.query,
			removeDayOffRule: dayOffRule.remove,
			getDayOffRule: dayOffRule.get,
			saveDayOffRule: dayOffRule.save,
			getFilterData: filterResult.query
		};

		return service;
	}
})();
