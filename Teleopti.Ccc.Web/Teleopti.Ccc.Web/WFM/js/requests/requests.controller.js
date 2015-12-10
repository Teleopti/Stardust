(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["RequestsToggles"];

	function requestsController(requestsToggles) {
		var vm = this;
		vm.onAgentSearchTermChanged = onAgentSearchTermChanged;

		requestsToggles.togglePromise.then(init);

		function init(toggles) {
			vm.isRequestsEnabled = toggles.isRequestsEnabled();			
			vm.isPeopleSearchEnabled = toggles.isPeopleSearchEnabled();
			vm.period = { startDate: new Date(), endDate: new Date() };
			vm.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false
			};
			vm.agentSearchTerm = vm.agentSearchOptions.keyword;
		}

		function onAgentSearchTermChanged(agentSearchTerm) {
			vm.agentSearchTerm = agentSearchTerm;
		}

	}

})();