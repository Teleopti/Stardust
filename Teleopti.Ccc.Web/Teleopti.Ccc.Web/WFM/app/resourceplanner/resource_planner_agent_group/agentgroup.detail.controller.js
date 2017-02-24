(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupsDetailController', Controller)
		.directive('planingPeriods', planingPeriodsDirective)
		.directive('dayoffRules', dayoffRulesDirective);

	Controller.$inject = ['$stateParams', 'agentGroupService', '$state'];

	function Controller($stateParams, agentGroupService, $state) {
		var vm = this;

	    var agentGroupId = $stateParams.groupId;
	    vm.agentGroup = {};
		vm.removeAgentGroup = removeAgentGroup;

		getAgentGroupbyId(agentGroupId);

		function getAgentGroupbyId(id){
		  var getAgentGroup = agentGroupService.getAgentGroupbyId({id:id});
			return getAgentGroup.$promise.then(function(data) {
				vm.agentGroup = data;
				return vm.agentGroup;
			});
		}

		function removeAgentGroup(agentGroup) {
			agentGroupService.removeAgentGroup({ id: agentGroup.Id }).$promise.then(function() {
				$state.go('resourceplanner.agentgroups');
			});
		}
	}

	function planingPeriodsDirective() {
		var directive = {
			restrict: 'EA',
      scope: {},
			templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.overview.html',
			controller: 'planningPeriodOverviewController as vm',
			bindToController: true
		};
		return directive;
	}

	function dayoffRulesDirective() {
		var directive = {
			restrict: 'EA',
      scope: {}, 
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
