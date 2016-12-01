(function() {
	var resourceplanner = angular.module('wfm.resourceplanner',
		[
			'restResourcePlannerService',
			'restPlanningPeriodService',
			'ui.router',
			'pascalprecht.translate',
			'wfm.notice',
			'toggleService',
			'scheduleManipulationService',
			'wfm.signalR',
			'wfm.utilities'
		]);

	resourceplanner.run([
		'$rootScope', '$state', '$location', function ($rootScope, $state, $location) {

			$rootScope.$on('$stateChangeSuccess',
				function (event, toState) {
					if ($location.url() === $state.current.url && toState.name === 'resourceplanner') $state.go('resourceplanner.overview');
				});

		}
	]);
})();
