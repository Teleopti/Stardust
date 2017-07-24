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
      var isSite = angular.isDefined(item.site);
      var isTeam = !isSite;

      if (isSite && item.isOpen)
        flipTeamSelected(item, item.isSelected);
      else if (isTeam) {
        var parentSite = ctrl.siteCards.find(function (site) { return site.site.Id === item.SiteId; });
        parentSite.isSelected = parentSite.teams.every(function (team) { return team.isSelected; });
      }
      ctrl.getSelectedItems(item);
    };

    function flipTeamSelected(site, selected) {
      site.teams.forEach(function (team) {
        team.isSelected = selected;
      })
    }

    ctrl.goToAgentsForTeam = function (team) {
      ctrl.openTeam(team);
    }

  }
})();