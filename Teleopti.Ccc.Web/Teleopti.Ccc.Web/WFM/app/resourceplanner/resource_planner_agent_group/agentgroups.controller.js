(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('agentGroupsController', Controller);

    Controller.$inject = ['$state','agentGroupService'];

    /* @ngInject */
    function Controller($state, agentGroupService) {
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
            $state.go('resourceplanner.oneagentroup', { groupId: groupId });
          }
        }
    }
})();
