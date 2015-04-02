'use strict';

angular
	.module('peopleSearchService', ['ngResource'])
	.service('PeopleSearch', [
	'$resource', function($resource) {
		this.search = $resource('../api/Search/People', { keyword: "@searchKey" }, {
			query: { method: 'GET', params: {  }, isArray: true }
		});
	}
]);