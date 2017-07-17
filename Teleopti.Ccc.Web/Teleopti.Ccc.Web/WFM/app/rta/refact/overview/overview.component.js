(function() {
'use strict';

  angular
    .module('wfm.rta')
    .component('rtaOverviewComponent', {
      templateUrl: 'app/rta/refact/overview/overview-component.html',
      controller: RtaOverviewComponentController,
      bindings: {
        siteCards: '=',
        agentsState: '=',
        getSelectedItems: '=',
        openTeam: '='
      },
    });

  RtaOverviewComponentController.inject = ['$state'];
  function RtaOverviewComponentController($state) {
    var ctrl = this;
    ctrl.selectedItems = {siteIds: []};
    ctrl.selectItem = function(item) {
      item.isSelected = !item.isSelected;
      ctrl.getSelectedItems(item);
    };

    ctrl.goToAgentsForTeam = function(team) {
      ctrl.openTeam(team);
    }

  }
})();