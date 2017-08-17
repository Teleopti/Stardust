(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsController', ['$state', 'Toggle', function($state, toggleSvc) {
		toggleSvc.togglesLoaded.then(function() {
			if (toggleSvc.Wfm_Requests_Refactoring_45470) {
				$state.go('requestsRefactor');
			} else {
				$state.go('requestsOrigin');
			}
		});
	}]);
})();