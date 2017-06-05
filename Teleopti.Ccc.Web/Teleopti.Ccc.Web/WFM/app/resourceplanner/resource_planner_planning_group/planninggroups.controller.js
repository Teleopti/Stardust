(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningGroupsController', Controller);

  Controller.$inject = ['$state', 'planningGroupService','planningGroups','localeLanguageSortingService'];

  function Controller($state, planningGroupService, planningGroups,localeLanguageSortingService) {
    var vm = this;

    vm.goForm = goForm;
    vm.goPlanGroup = goPlanGroup;
    vm.goEditPlanGroup = goEditPlanGroup;
    vm.goDayOffRule = goDayOffRule;
    vm.planningGroups = planningGroups.sort(localeLanguageSortingService.localeSort('+Name'));
    vm.hasAg = hasAg;

    function hasAg() {
      if (vm.planningGroups.length > 0)
        return true;
    }

    function goForm() {
      $state.go('resourceplanner.createplanninggroup');
    }

    function goEditPlanGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.editplanninggroup', { groupId: groupId });
      }
    }

    function goPlanGroup(groupId) {
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
