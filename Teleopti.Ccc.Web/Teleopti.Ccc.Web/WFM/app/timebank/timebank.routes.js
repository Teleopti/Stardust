(function() {
  'use strict';

  angular
  .module('wfm.timebank')
  .config(stateConfig);

  function stateConfig($stateProvider) {
   $stateProvider.state('timebank', {
     url: '/timebank',
     templateUrl: 'app/timebank/timebank.html',
     controller: 'TimebankController as vm'
   })
 }
})();
