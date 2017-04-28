(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupsDetailController', Controller)
		.directive('agentgroupDetail', agentgroupDetailDirective);

	Controller.$inject = ['$stateParams', 'agentGroupService'];

	function Controller($stateParams, agentGroupService) {
		var vm = this;
	}
	function agentgroupDetailDirective() {
		var directive = {
			restrict: 'EA',
			scope: {
				agentGroup: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_agent_group/agentgroup.detail.html',
			controller: 'agentGroupsDetailController as vm',
			bindToController: true
		};
		return directive;
	}
})();
