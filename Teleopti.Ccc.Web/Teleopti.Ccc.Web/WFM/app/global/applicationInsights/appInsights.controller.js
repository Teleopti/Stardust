(function() {
	'use strict';

	angular
		.module('wfm.ai')
		.controller('AppInsightsController', AppInsightsController);

	AppInsightsController.$inject = ['$scope', '$rootScope', '$state'];

	function AppInsightsController($scope, $rootScope, $state) {
		var vm = this;
		trackPage();

		function trackPage() {
		$rootScope.$on('$stateChangeSuccess', function () {
			appInsights.trackPageView($state.current.name);
			});
		}
	}
})();
