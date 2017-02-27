(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodOverviewController', Controller);

    Controller.$inject = ['$stateParams','$state','planningPeriodService'];

    function Controller( $stateParams, $state, planningPeriodService) {
        var vm = this;
        var agentGroupId = $stateParams.groupId;
        vm.planningPeriods = [];
        vm.startNextPlanningPeriod = startNextPlanningPeriod;
        vm.showPlanningPeriod = showPlanningPeriod;

        getPlanningPeriod(agentGroupId);

        function getPlanningPeriod(id){
          if (id) {
          	var query = planningPeriodService.getPlanningPeriods({ agentGroupId: id });
          	return query.$promise.then(function (data) {
              vm.planningPeriods = data;

              return vm.planningPeriods;
            });
          }
        }

        function startNextPlanningPeriod() {
          var id = agentGroupId;
          var nextPlanningPeriod = planningPeriodService.nextPlanningPeriod({ agentGroupId: id });
          return nextPlanningPeriod.$promise.then(function (data) {
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

        function showPlanningPeriod(planningPeriodId) {
        	if (planningPeriodId) {
        		$state.go('resourceplanner.planningperiod', { id: planningPeriodId });
	        }
        }
    }
})();
