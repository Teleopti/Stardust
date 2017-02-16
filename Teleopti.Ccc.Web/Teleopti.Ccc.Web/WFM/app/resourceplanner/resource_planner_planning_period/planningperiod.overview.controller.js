(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodOverviewController', Controller);

    Controller.$inject = ['$stateParams','$state','agentGroupService','planningPeriodService'];

    /* @ngInject */
    function Controller( $stateParams, $state, agentGroupService, planningPeriodService) {
        var vm = this;
        var agentGroupId = $stateParams.groupId;
        vm.agentGroup = {};
        vm.planningPeriods = [];
        vm.startNextPlanningPeriod = startNextPlanningPeriod;

        getAgentGroupbyId(agentGroupId);
        getPlanningPeriod(agentGroupId);

        function getAgentGroupbyId(id){
          var getAgentGroup = agentGroupService.getAgentGroupbyId.get({id:id});
    			return getAgentGroup.$promise.then(function(data) {
    				vm.agentGroup = data;
    				return vm.agentGroup;
    			});
        }

        function getPlanningPeriod(id){
          var getPlanningPeriod = planningPeriodService.getPlanningPeriodByAgentGroupId.query({agentGroupId:id});
          return getPlanningPeriod.$promise.then(function(data){
            vm.planningPeriods = data;
            return vm.planningPeriods;
          });
        }

        function startNextPlanningPeriod() {
          var id = agentGroupId;
          var getNextPlanningPeriod = planningPeriodService.getNextPlanningPeriod.save({agentGroupId:id});
          return getNextPlanningPeriod.$promise.then(function(data){
            vm.planningPeriods.push(data);
            goAgentGroup(agentGroupId);
            return vm.planningPeriods;
          });
        }

        function goAgentGroup(groupId){
          if (groupId) {
            $state.go('resourceplanner.oneagentroup', { groupId: groupId });
          }
        }
    }
})();
