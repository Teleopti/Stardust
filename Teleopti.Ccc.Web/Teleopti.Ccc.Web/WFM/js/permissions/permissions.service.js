(function() {
	'use strict';

	var permissionsService = angular.module('wfm.permissions');
	permissionsService.service('PermissionsService', [
		'$resource', function($resource) {
			this.roles = $resource('../api/Permissions/Roles', {}, {
				get: { method: 'GET', params: {}, isArray: true },
				post: { method: 'POST', params: name }
			});

			this.rolesPermissions = $resource('../api/Permissions/Roles/:Id', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});

			this.applicationFunctions = $resource('../api/Permissions/ApplicationFunctions', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});

			this.organizationSelections = $resource('../api/Permissions/OrganizationSelection', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});

			this.duplicateRole = $resource('../api/Permissions/Roles/:Id/Copy', { Id: "@Id" }, {
				query: { method: 'POST', params: {}, isArray: false }
			});

			this.manageRole = $resource('../api/Permissions/Roles/:Id', { Id: "@Id" }, {
				deleteRole: { method: 'DELETE', params: {}, isArray: false },
				update: { method: 'PUT', params: { newDescription: {} }, isArray: false }
			});

			this.postFunction = $resource('../api/Permissions/Roles/:Id/Functions', { Id: "@Id" }, {
				query: { method: 'POST', params: { Functions: [] }, isArray: true }
			});

			this.deleteFunction = $resource('../api/Permissions/Roles/:Id/Function/:FunctionId', { Id: "@Id", FunctionId: "@FunctionId" }, {
				query: { method: 'DELETE', params: {}, isArray: false }
			});

			this.deleteAllFunction = $resource('../api/Permissions/Roles/:Id/DeleteFunction/:FunctionId', { Id: "@Id", FunctionId: "@FunctionId" }, {
				query: { method: 'POST', params: { Functions: [] }, isArray: false }
			});
			this.deleteAllData = $resource('../api/Permissions/Roles/:Id/DeleteData', { Id: "@Id" }, {
				query: { method: 'POST', params: { Data: [] }, isArray: false }
			});
			
			this.deleteAvailableData = $resource('../api/Permissions/Roles/:Id/AvailableData/:Type/:DataId', { Id: "@Id", Type: '@Type', DataId: "@DataId" }, {
				query: { method: 'DELETE', params: {}, isArray: false }
			});


			this.assignOrganizationSelection = $resource('../api/Permissions/Roles/:Id/AvailableData', { Id: "@Id" }, {
				postData: { method: 'POST', params: { BusinessUnits: [], Sites: [], Teams: [], RangeOption: [] }, isArray: true }
			});

		}
	]);
})();