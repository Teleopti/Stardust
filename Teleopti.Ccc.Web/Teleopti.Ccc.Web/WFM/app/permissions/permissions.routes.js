(function() {
  'use strict';

  angular
  .module('wfm.permissions')
  .config(stateConfig);

  // function stateConfig($stateProvider) {
  //   $stateProvider.state('permissions', {
	// 		url: '/permissions',
	// 		templateUrl: 'app/permissions/refact/permissions-refact.html',
	// 		controller: 'PermissionsCtrlRefact as vm'
	// 	})
  // }

   function stateConfig($stateProvider) {
    $stateProvider.state('permissions', {
			url: '/permissions',
			templateUrl: 'app/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		})
  }

})();
