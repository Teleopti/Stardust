(function () {
  'use strict';

  angular
  .module('wfm.permissions')
  .config(stateConfig);
  function stateConfig($stateProvider) {
    $stateProvider
    .state('permissions', {
      url: '/permissions?open',
      templateUrl: 'app/permissions/refact/permissions-refact.html',
      controller: 'PermissionsRefactController as vm'
    })
  }
})();
