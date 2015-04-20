(function() {
	'use strict';
	angular.module('restSearchService', ['ngResource'])
		.service('SearchSvrc', [
			'$resource', function($resource) {
				this.search = $resource('../api/Search/Global', { keyword: "@searchKey" }, {
					query: { method: 'GET', params: {}, isArray: true }
				});
			}
		]);
})();