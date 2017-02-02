(function() {
'use strict';

  angular
    .module('wfm.timebank')
    .service('timebankAdminService', timebankAdminService);

  timebankAdminService.inject = ['timebankService'];
  function timebankAdminService(timebankService) {
    
    this.getPeople = getPeople;

    function getPeople(){
      return timebankService.getPeople();
    };

  }

})();