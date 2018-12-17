(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('PlanGroupSettingService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {
		var setting = $resource('../api/resourceplanner/plangroupsetting/:id', { id: '@id' });
		var filterResult = $resource('../api/filters', { searchString: '@searchString', maxHits: 100 });

		var service = {
			removeSetting: setting.remove,
			saveSetting: setting.save,
			getFilterData: filterResult.query
		};

		return service;
	}
})();
