
'use strict';

var areaService = angular.module('restAreasService', ['ngResource']);
areaService.service('AreasSvrc', [
	'$resource', 
	function($resource) {
		var service = {};
		var areas;

		service.getAreas = function(){
			if(!areas){
				areas = getAreasFromServer.query({}).$promise;
			}
			return areas;
		}

		var getAreasFromServer = $resource('../api/Global/Application/Areas', {}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});
		return service;
	}
]);
