(function() {
'use strict';

  angular
    .module('wfm.timebank')
    .component('admin', {
      templateUrl: 'app/timebank/admin/timebank-admin-component.html',
      controller: TimebankAdminController,
      bindings: {
      },
    });

  TimebankAdminController.inject = [];
  function TimebankAdminController() {
    var ctrl = this;
    


    ctrl.onInit = function() { };
    ctrl.onChanges = function(changesObj) { };
    ctrl.onDestory = function() { };
  }
})();