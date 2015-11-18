(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.service('ResourcePlannerFilterSrvc', [
		'$resource', function ($resource) {
            this.getData = $resource('../api/filters?searchString=:searchString&maxHits=:maxHits', { searchString : "@searchString", maxHits : "@maxHits"}, {
                query: { method: 'GET', params: {}, isArray: true }
            });
		}
])
})();
