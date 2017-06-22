(function() {
'use strict';

  angular
    .module('wfm.rta')
    .component('rtaOverviewComponent', {
      templateUrl: 'app/rta/refact/overview/overview-component.html',
      controller: RtaOverviewComponentController,
      bindings: {
        siteCards: '=',
      },
    });

  RtaOverviewComponentController.inject = [];
  function RtaOverviewComponentController() {
    var ctrl = this;
    
  }
})();