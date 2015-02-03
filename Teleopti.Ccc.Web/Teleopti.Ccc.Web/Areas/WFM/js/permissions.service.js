var permissionsService = angular.module('permissionsService', ['ngResource']);
permissionsService.factory('Roles', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles', {}, {
		get: { method: 'GET', params: {}, isArray: true },
		post: { method: 'POST', params: name }
	});
}]);

permissionsService.factory('RolesPermissions', ['$resource', function ($resource) {
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
	return $resource('../../api/Permissions/Roles/:Id/Copy', { Id: "@Id" }, {
		query: { method: 'POST', params: { }, isArray: false }
	});
}]);

permissionsService.factory('AssignFunction', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id/Functions', { Id: "@Id" }, {
		postFunctions: { method: 'POST', params: { Functions: [] }, isArray: false },
		deleteFunctions: { method: 'DELETE', params: { Functions: [] }, isArray: false }
	});
}]);

permissionsService.factory('AssignData', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id/AvailableData', { Id: "@Id" }, {
		postData: { method: 'POST', params: { BusinessUnits: [], Sites: [], Teams: [], People: [], RangeOption: [] }, isArray: false },
		deleteData: { method: 'DELETE', params: { BusinessUnits: [], Sites: [], Teams: [], People: [], RangeOption: [] }, isArray: false }
	});
}]);

permissionsService.factory('ManageRole', ['$resource', function ($resource) {
	return $resource('../../api/Permissions/Roles/:Id', {Id: "@Id"}, {
		deleteRole: { method: 'DELETE', params: {}, isArray: false },
		update: { method: 'PUT', params: { newDescription: {} }, isArray: false }
	});
}]);