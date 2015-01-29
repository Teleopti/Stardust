var permissionsService = angular.module('permissionsService', ['ngResource']);
permissionsService.factory('Roles', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles', {}, {
		query: { method: 'GET', params: { }, isArray: true }
	});
}]);

permissionsService.factory('ApplicationFunctions', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/ApplicationFunctions', {}, {
		query: { method: 'GET', params: {}, isArray: true }
	});
}]);