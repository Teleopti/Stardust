(function() {
	'use strict';

	angular
		.module('wfm.ai')
		.controller('AppInsightsController', AppInsightsController);

	AppInsightsController.$inject = ['$scope', '$rootScope', '$state', '$window'];

	function AppInsightsController($scope, $rootScope, $state, $window) {
		$rootScope.$on('$stateChangeSuccess',
			function() {
				if ($window.appInsights)
					$window.appInsights.trackPageView($state.current.name);
			});
	}
})();
