'use strict';

var permissionsService = angular.module('restService', ['ngResource']);
permissionsService.service('Outbound', ['$resource', function ($resource) {
	//this.roles = $resource('../api/Permissions/Roles', {}, {
	//	get: { method: 'GET', params: {}, isArray: true },
	//	post: { method: 'POST', params: name }
	//});

}]);