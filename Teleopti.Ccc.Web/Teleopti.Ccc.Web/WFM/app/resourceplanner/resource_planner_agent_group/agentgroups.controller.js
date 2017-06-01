(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('agentGroupsController', Controller);

  Controller.$inject = ['$state', 'agentGroupService','agentGroups','localeLanguageSortingService'];

  function Controller($state, agentGroupService, agentGroups,localeLanguageSortingService) {
    var vm = this;

    vm.goForm = goForm;
    vm.goAgentGroup = goAgentGroup;
    vm.goEditAgentGroup = goEditAgentGroup;
    vm.goDayOffRule = goDayOffRule;
    vm.agentGroups = agentGroups.sort(localeLanguageSortingService.localeSort('+Name'));
    vm.hasAg = hasAg;

    function hasAg() {
      if (vm.agentGroups.length > 0)
        return true;
    }

    function goForm() {
      $state.go('resourceplanner.createagentgroup');
    }

    function goEditAgentGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.editagentgroup', { groupId: groupId });
      }
    }

    function goAgentGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.selectplanningperiod', { groupId: groupId });
      }
    }

    function goDayOffRule(groupId) {
      if (groupId) {
        $state.go('resourceplanner.dayoffrulesoverview', { groupId: groupId });
      }
    }
  }
})();
