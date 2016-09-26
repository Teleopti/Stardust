(function() {
  'use strict';

  angular
  .module('wfm.permissions')
  .config(stateConfig);

  function stateConfig($stateProvider) {
    $stateProvider.state('permissions', {
			url: '/permissions',
			templateUrl: 'js/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		})
  }
})();
