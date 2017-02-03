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

  TimebankAdminController.inject = ['timebankAdminService'];
  function TimebankAdminController(timebankAdminService) {
    var ctrl = this;
    ctrl.people = [];
    ctrl.contracts = [];

    ctrl.getContracts = function () {
      ctrl.contracts = timebankAdminService.getContracts();
    }
    ctrl.getPeople = function () {
      ctrl.people = timebankAdminService.getPeople();
    }

    ctrl.getPeople();
    ctrl.getContracts();
    ctrl.onChanges = function(changesObj) { };
    ctrl.onDestory = function() { };
  }
})();