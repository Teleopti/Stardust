(function() {
    'use strict';

    angular
        .module('wfm.permissions')
        .factory('PermissionsServiceRefact', PermissionsServiceRefact);

    PermissionsServiceRefact.$inject = ['$resource'];

    function PermissionsServiceRefact($resource) {

      //ROLES
      var roles = $resource('../api/Permissions/Roles', {});
      var manage = $resource('../api/Permissions/Roles/:Id', { Id: "@Id" }, {
          deleteRole: { method: 'DELETE', params: {}, isArray: false },
				  update: { method: 'PUT', params: { newDescription: {} }, isArray: false },
          getRoleInformation: {method: 'GET', isArray: false}
			});
      var copyRole = $resource('../api/Permissions/Roles/:Id/Copy', { Id: "@Id" }, {
				copy: { method: 'POST', params: {}, isArray: false }
			});
      //END OF ROLES

      //FUNCTIONS
      var applicationFunctions = $resource('../api/Permissions/ApplicationFunctions', {});
      //END OF FUNCTIONS

      //ORGANIZATION
      	var organizationSelection	= $resource('../api/Permissions/OrganizationSelection', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
      //END OF ORGANIZATION

      var service = {
        roles: roles,
        manage: manage,
        copyRole: copyRole,
        applicationFunctions: applicationFunctions,
        organizationSelection: organizationSelection
      };

      return service;
    }
})();
