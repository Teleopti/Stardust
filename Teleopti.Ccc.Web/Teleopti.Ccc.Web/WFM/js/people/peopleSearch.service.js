'use strict';

angular
	.module('peopleSearchService', ['ngResource'])
	.service('PeopleSearch', [
	'$resource', function($resource) {
	    this.search = $resource('../api/Search/People', { keyword: "@searchKey", pageSize: "@pageSize", currentPageIndex: "@currentPageIndex" }, {
		    query: { method: 'GET', params: { }, isArray: false }
		});
	}
]);