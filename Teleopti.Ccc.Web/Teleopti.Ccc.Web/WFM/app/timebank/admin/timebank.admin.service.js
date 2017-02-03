(function() {
'use strict';

  angular
    .module('wfm.timebank')
    .service('timebankAdminService', timebankAdminService);

  timebankAdminService.inject = ['timebankService'];
  function timebankAdminService(timebankService) {
    
    this.getPeople = getPeople;
    this.getContracts = getContracts;

    function getPeople() {
      return timebankService.getPeople();
    }

    function getContracts() {
      return timebankService.getContracts();
    }

  }

})();