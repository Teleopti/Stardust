var permissionsService = angular.module('permissionsService', ['ngResource']);
permissionsService.factory('Roles', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles', {}, {
		query: { method: 'GET', params: { }, isArray: true }
	});
}]);

permissionsService.factory('RolesFunctions', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id', {}, {
		query: { method: 'GET', params: {}, isArray: false }
	});
}]);

permissionsService.factory('ApplicationFunctions', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/ApplicationFunctions', {}, {
		query: { method: 'GET', params: { }, isArray: true }
	});
}]);

permissionsService.factory('OrganizationSelections', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/OrganizationSelection', {}, {
		query: { method: 'GET', params: { }, isArray: false }
	});
}]);

permissionsService.factory('DuplicateRole', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id/Copy', {}, {
		query: { method: 'POST', params: { }, isArray: false }
	});
}]);

permissionsService.factory('DeleteRole', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id', {}, {
		query: { method: 'DELETE', params: {}, isArray: false }
	});
}]);

permissionsService.factory('AddRole', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles', {}, {
		query: { method: 'POST', params: name, isArray: false }
	});
}]);

