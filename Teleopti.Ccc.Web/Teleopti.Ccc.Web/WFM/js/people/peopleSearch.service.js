'use strict';

angular
	.module('peopleSearchService', ['ngResource'])
	.service('PeopleSearch', [
		'$resource', function($resource) {
			this.search = $resource('../api/Search/People/Keyword', {
				keyword: "@searchKey",
				pageSize: "@pageSize",
				currentPageIndex: "@currentPageIndex",
				sortedColumns: "@sortedColumn"
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: false
				}
			});

			this.isFeatureEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle',
			{
				toggle: "@toggle"
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: false
				}
			});

			this.importUsers = $resource('../api/People/ImportPeople', {}, {
				post: {
					method: 'POST',
					params: {},
					isArray: true
				},

			});
		}
	]);