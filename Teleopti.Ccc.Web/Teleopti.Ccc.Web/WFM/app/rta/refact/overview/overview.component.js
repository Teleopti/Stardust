(function () {
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
        openTeam: '=',
        selectedItems: '='
      },
    });

  RtaOverviewComponentController.inject = [];
  function RtaOverviewComponentController() {
    var ctrl = this;
    ctrl.selectItem = function (item) {
      item.isSelected = !item.isSelected;
      var itemIsSite = angular.isDefined(item.site);
      
      if (itemIsSite && item.isSelected && item.isOpen) {
        item.teams.forEach(function (team) {
          team.isSelected = true;
        })
      }
      else if (itemIsSite && !item.isSelected && item.isOpen) {
        item.teams.forEach(function (team) {
          team.isSelected = false;
        })
      }
      else if (!itemIsSite && item.isSelected) {
        var match = ctrl.siteCards.find(function (site) {
          return site.site.Id === item.SiteId;
        });
        var allTeamsAreSelected = match.teams.every(function (team) {
          return team.isSelected;
        });
        if (allTeamsAreSelected) match.isSelected = true; 
      }
      else if (!itemIsSite && !item.isSelected) {
        var match = ctrl.siteCards.find(function (site) {
          return site.site.Id === item.SiteId;
        });
        match.isSelected = false;
      }
      ctrl.getSelectedItems(item);
    };

    ctrl.goToAgentsForTeam = function (team) {
      ctrl.openTeam(team);
    }

  }
})();