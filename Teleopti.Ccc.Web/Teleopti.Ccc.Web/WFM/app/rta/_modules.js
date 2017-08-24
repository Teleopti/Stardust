(function () {
	angular
		.module('wfm.rta', [
			'ui.grid',
			'ui.grid.autoResize',
			'ui.grid.resizeColumns',
			'ui.grid.selection',
			'ngResource',
			'ui.router',
			'ngStorage',
			'toggleService',
			'wfm.notice',
			'pascalprecht.translate',
			'currentUserInfoService',
			'localeLanguageSortingService',
			'wfm.helpingDirectives'
		])
		.run(runRtaModule);

	runRtaModule.$inject = ['$rootScope', '$state', '$location', 'Toggle'];

	function runRtaModule($rootScope, $state, $location, Toggle) {
		var toggles = {};

		Toggle.togglesLoaded.then(function () {
			toggles = Toggle
		});

		var result = $rootScope.$on('$stateChangeSuccess',
			function (event, toState) {
				var location = $location.url();
				if (location == $state.current.url && toState.name == 'rta') {
					if (toggles.RTA_FrontEndRefactor_44772) {
						$state.go('refact-rta')
					} else {
						$state.go('rta.sites')
					}
				}

			});
		return result;
	}
})();
