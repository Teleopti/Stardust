'use strict';

var searchService = angular.module('restSearchService', ['ngResource']);
searchService.service('SearchSvrc', [
	'$resource', function($resource) {
		this.search = $resource('../api/Search/People', { keyword: "@searchKey" }, {
			query: { method: 'GET', params: {  }, isArray: true }
		});
	}
]);