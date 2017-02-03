(function () {
  'use strict';

  angular
    .module('wfm.timebank')
    .controller('TimebankController', TimebankController);

  TimebankController.inject = ['timebankService', 'Toggle'];
  function TimebankController(timebankService, Toggle) {
    var vm = this;
    Toggle.togglesLoaded.then(function () {
      if (!Toggle.Wfm_Timebank_GUI_42861) {
        $location.path('#/')
      }
    });


  }
})();