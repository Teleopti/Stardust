(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('agentGroupsController', Controller)
    .directive('agentGroups', agentGroupsDirective);

  Controller.$inject = ['$state', 'agentGroupService'];

  function Controller($state, agentGroupService) {
    var vm = this;

    vm.goForm = goForm;
    vm.goAgentGroup = goAgentGroup;
    vm.goEditAgentGroup = goEditAgentGroup;
    vm.goDayOffRule = goDayOffRule;
    vm.agentGroups = undefined;

    getAgentGroups();

    function getAgentGroups() {
      var query = agentGroupService.getAgentGroups();
      return query.$promise.then(function (data) {
        vm.agentGroups = data;
        return vm.agentGroups;
      });
    }

    function goForm() {
      $state.go('resourceplanner.createagentgroup');
    }

    function goEditAgentGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.editagentgroup', { groupId: groupId});
      }
    }

    function goAgentGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.selectplanningperiod', { groupId: groupId});
      }
    }

    function goDayOffRule(groupId) {
      if (groupId) {
        $state.go('resourceplanner.dayoffrulesoverview', { groupId: groupId });
      }
    }
  }

  function agentGroupsDirective() {
    var directive = {
      restrict: 'EA',
      scope: {},
      templateUrl: 'app/resourceplanner/resource_planner_agent_group/agentgroups.html',
      controller: 'agentGroupsController as vm',
      bindToController: true
    };
    return directive;
  }
})();
