(function() {
'use strict';

  angular
    .module('wfm.rta')
    .component('rtaOverviewComponent', {
      templateUrl: 'app/rta/refact/overview/overview-component.html',
      controller: RtaOverviewComponentController,
      bindings: {
        siteCards: '=',
        agentsState: '='
      },
    });

  RtaOverviewComponentController.inject = [];
  function RtaOverviewComponentController() {
    var ctrl = this;
    ctrl.selectItem = function(card) {
      card.isSelected = !card.isSelected;
    }
  }
})();