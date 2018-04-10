(function() {
	'use strict';

	angular
		.module('wfm.ai')
		.controller('AppInsightsController', AppInsightsController);

	AppInsightsController.$inject = ['$scope', '$rootScope', '$state'];

	function AppInsightsController($scope, $rootScope, $state) {
		$rootScope.$on('$stateChangeSuccess',
			function() {
				if (appInsights)
					appInsights.trackPageView($state.current.name);
			});
	}
})();
