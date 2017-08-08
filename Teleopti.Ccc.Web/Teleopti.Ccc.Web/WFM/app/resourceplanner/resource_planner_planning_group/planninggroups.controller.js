(function () {
  'use strict';

  angular
    .module('wfm.resourceplanner')
    .controller('planningGroupsController', Controller);

  Controller.$inject = ['$state', 'planningGroupService', 'planningGroups', 'localeLanguageSortingService'];

  function Controller($state, planningGroupService, planningGroups, localeLanguageSortingService) {
    var vm = this;

    vm.goForm = goForm;
    vm.goPlanningGroup = goPlanningGroup;
    vm.goEditPlanningGroup = goEditPlanningGroup;
    vm.goDayOffRule = goDayOffRule;
    vm.planningGroups = planningGroups ? planningGroups.sort(localeLanguageSortingService.localeSort('+Name')) : [];
    vm.hasPg = hasPg;

    function hasPg() {
      if (vm.planningGroups.length > 0)
        return true;
    }

    function goForm() {
      $state.go('resourceplanner.createplanninggroup');
    }

    function goEditPlanningGroup(groupId) {
      if (groupId) {
        $state.go('resourceplanner.editplanninggroup', { groupId: groupId });
      }
    }

    function goPlanningGroup(groupId) {
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
