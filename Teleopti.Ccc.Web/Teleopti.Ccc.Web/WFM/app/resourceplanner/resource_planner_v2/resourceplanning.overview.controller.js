(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('resourceplanningOverviewController', Controller);

	Controller.$inject = ['$stateParams', 'agentGroupService', '$state'];

	function Controller($stateParams, agentGroupService, $state) {
		var vm = this;
	}
})();