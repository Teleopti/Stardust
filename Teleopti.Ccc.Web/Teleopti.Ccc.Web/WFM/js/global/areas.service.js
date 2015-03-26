'use strict';

var areaService = angular.module('restAreasService', ['ngResource']);
areaService.service('AreasSvrc', [
	'$resource', function ($resource) {
		this.getAreas = $resource('../api/Global/Application/Areas', {}, {
			query: { method: 'GET', params: {}, isArray: true }
		});

		this.getFilters = $resource('../api/ResourcePlanner/Filter', {}, {
			query: { method: 'GET', params: {}, isArray: true }
		});
	}
]);