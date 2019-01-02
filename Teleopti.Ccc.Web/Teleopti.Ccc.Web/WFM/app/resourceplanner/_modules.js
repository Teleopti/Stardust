(function() {
	angular
	.module('wfm.resourceplanner',
		[
			'ui.router',
			'pascalprecht.translate',
			'wfm.notice',
			'toggleService',
			'scheduleManipulationService',
			'wfm.signalR',
			'wfm.utilities'
		])
		.run(runResourcePlanner);

runResourcePlanner.$inject = ['$rootScope', '$state', '$location'];

function runResourcePlanner($rootScope, $state, $location) {
	var result = $rootScope.$on('$stateChangeSuccess',
		function (event, toState) {
			if ($location.url() === $state.current.url && toState.name === 'resourceplanner')
				$state.go('resourceplanner.overview');
		});
	return result;
}
})();
