'use strict';

var filterService = angular.module('restFilterService', ['ngResource']);
filterService.service('FilterSvrc', [
	'$resource', function ($resource) {
		this.getFilters = $resource('../api/ResourcePlanner/Filter', {}, {
			query: { method: 'GET', params: {}, isArray: true }
		});
	}
]);