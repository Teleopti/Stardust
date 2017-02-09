(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('agentGroupsController', Controller);

    Controller.$inject = ['$state','agentGroupService','resourcePlannerRouteService'];

    /* @ngInject */
    function Controller($state, agentGroupService, resourcePlannerRouteService) {
        var vm = this;

        vm.goForm = goForm;
        vm.goAgentGroup = goAgentGroup;
        vm.agentGroups = [];

        getAgentGroups();

        function getAgentGroups(){
          var getAgentGroups = agentGroupService.getAgentGroups.query();
    			return getAgentGroups.$promise.then(function(data) {
    				vm.agentGroups = data;
    				return vm.agentGroups;
    			});
        }

        function goForm(){
          $state.go('resourceplanner.createagentgroup');
        }

        function goAgentGroup(groupId){
          if (groupId) {
            resourcePlannerRouteService.goToAgentGroup(groupId);
          }
        }
    }
})();
