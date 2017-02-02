(function() {
'use strict';

  angular
    .module('wfm.timebank')
    .controller('TimebankController', TimebankController);

  TimebankController.inject = ['timebankService'];
  function TimebankController(timebankService) {
    var vm = this;
    

  }
})();