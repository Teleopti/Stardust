(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupsDetailController', Controller);

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
})();
