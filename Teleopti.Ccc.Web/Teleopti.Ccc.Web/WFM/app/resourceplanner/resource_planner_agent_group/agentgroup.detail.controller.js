(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupsDetailController', Controller)
		.directive('agentgroupDetail', agentgroupDetailDirective);

	Controller.$inject = ['$stateParams', 'agentGroupService', '$state'];

	function Controller($stateParams, agentGroupService, $state) {
		var vm = this;

		var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;
		vm.agentGroup = {};

		getAgentGroupbyId(agentGroupId);

		function getAgentGroupbyId(id) {
			if (id !== null) {
				var getAgentGroup = agentGroupService.getAgentGroupbyId({ id: id });
				return getAgentGroup.$promise.then(function (data) {
					vm.agentGroup = data;
					return vm.agentGroup;
				});
			}
		}
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
