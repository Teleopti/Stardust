(function() {
    'use strict';

    angular
        .module('wfm.permissions')
        .factory('PermissionsServiceRefact', PermissionsServiceRefact);

    PermissionsServiceRefact.$inject = ['$resource'];

    function PermissionsServiceRefact($resource) {
      var roles = $resource('../api/Permissions/Roles', {});

      var manage = $resource('../api/Permissions/Roles/:Id', { Id: "@Id" }, {
          deleteRole: { method: 'DELETE', params: {}, isArray: false },
				  update: { method: 'PUT', params: { newDescription: {} }, isArray: false }
			});

      var service = {
        roles: roles,
        manage: manage
      };

      return service;
    }
})();
