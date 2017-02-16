(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupsDetailController', Controller)
		.directive('planingPeriods', planingPeriodsDirective)
		.directive('dayoffRules', dayoffRulesDirective);

	Controller.$inject = ['$stateParams', 'agentGroupService'];

	function Controller($stateParams, agentGroupService) {
		var vm = this;

    var agentGroupId = $stateParams.groupId;
		vm.agentGroup = {};

		getAgentGroupbyId(agentGroupId);

		function getAgentGroupbyId(id){
		  var getAgentGroup = agentGroupService.getAgentGroupbyId.get({id:id});
			return getAgentGroup.$promise.then(function(data) {
				vm.agentGroup = data;
				return vm.agentGroup;
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
