(function () {
    'use strict';

    angular
        .module('wfm.permissions')
        .factory('PermissionsServiceRefact', PermissionsServiceRefact);

    PermissionsServiceRefact.$inject = ['$resource'];

    function PermissionsServiceRefact($resource) {

        var roles = $resource('../api/Permissions/Roles', {});
        var manage = $resource('../api/Permissions/Roles/:Id', { Id: "@Id" }, {
            deleteRole: { method: 'DELETE', params: {}, isArray: false },
            update: { method: 'PUT', params: { newDescription: {} }, isArray: false },
            getRoleInformation: { method: 'GET', isArray: false }
        });
        var copyRole = $resource('../api/Permissions/Roles/:Id/Copy', { Id: "@Id" }, {
            copy: { method: 'POST', params: {}, isArray: false }
        });

        var applicationFunctions = $resource('../api/Permissions/ApplicationFunctions', {});
        var postFunctions = $resource('../api/Permissions/Roles/:Id/Functions', { Id: "@Id" }, {
            query: { method: 'POST', params: { Functions: [] }, isArray: true }
        });

        var deleteAllFunction = $resource('../api/Permissions/Roles/:Id/DeleteFunction/:FunctionId', { Id: "@Id", FunctionId: "@FunctionId" }, {
            delete: { method: 'POST', params: { Functions: [] }, isArray: false }
        });

        var deleteFunction = $resource('../api/Permissions/Roles/:Id/Function/:FunctionId', { Id: "@Id", FunctionId: "@FunctionId" }, {
            delete: { method: 'DELETE', params: {}, isArray: false }
        });

        var organizationSelection = $resource('../api/Permissions/OrganizationSelection', {}, {
            query: { method: 'GET', params: {}, isArray: false }
        });

        var assignOrganizationSelection = $resource('../api/Permissions/Roles/:Id/AvailableData', { Id: "@Id" }, {
            postData: { method: 'POST', params: { BusinessUnits: [], Sites: [], Teams: [], RangeOption: [] }, isArray: true }
        });

        var deleteAllData = $resource('../api/Permissions/Roles/:Id/DeleteData', { Id: "@Id" }, {
            delete: { method: 'POST', params: { Data: [] }, isArray: false }
        });

        var deleteAvailableData = $resource('../api/Permissions/Roles/:Id/AvailableData/:Type/:DataId', { Id: "@Id", Type: '@Type', DataId: "@DataId" }, {
            delete: { method: 'DELETE', params: {}, isArray: false }
        });

        var service = {
            roles: roles,
            manage: manage,
            copyRole: copyRole,
            postFunctions: postFunctions,
            deleteAllFunction: deleteAllFunction,
            deleteFunction: deleteFunction,
            applicationFunctions: applicationFunctions,
            organizationSelection: organizationSelection,
            assignOrganizationSelection: assignOrganizationSelection,
            deleteAllData: deleteAllData,
            deleteAvailableData: deleteAvailableData
        };

        return service;
    }
})();
