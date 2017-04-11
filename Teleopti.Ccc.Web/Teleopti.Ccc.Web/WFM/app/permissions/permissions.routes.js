(function () {
  'use strict';

  angular
    .module('wfm.permissions')
    .config(stateConfig);
  function stateConfig($stateProvider) {
    $stateProvider
      .state('permissions', {
        url: '/permissions',
        resolve: {
          toggles: function (Toggle) {
	          return Toggle;
          }
        },
        controller: function ($state, toggles) {
          if (toggles.WfmPermission_RelaseRefactor_43587) {
            $state.go('permissions-new');
          } else {
	          $state.go('permissions-old');
          }
        }
      })
      .state('permissions-old', {
        templateUrl: 'app/permissions/permissions.html',
        controller: 'PermissionsCtrl'
      })
      .state('permissions-new', {
        templateUrl: 'app/permissions/refact/permissions-refact.html',
        controller: 'PermissionsRefactController as vm'
      })
  }
})();




