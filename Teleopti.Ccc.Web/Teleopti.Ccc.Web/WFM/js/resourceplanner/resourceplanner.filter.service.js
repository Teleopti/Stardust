(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.service('ResourcePlannerFilterSrvc', [
		'$resource', function ($resource) {
            this.getData = $resource('../api/filters?searchString=:searchString&maxHits=10', { searchString : "@searchString"}, {
                query: { method: 'GET', params: {}, isArray: true },
            });
		}
])
})();
