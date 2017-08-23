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

	runRtaModule.$inject = ['$rootScope', '$state', '$location', 'Toggle', 'rtaStateStorageService'];

	function runRtaModule($rootScope, $state, $location, Toggle, rtaStateStorageService) {
		var toggles = {};

		Toggle.togglesLoaded.then(function () {
			toggles = Toggle
		});

		var result = $rootScope.$on('$stateChangeSuccess',
			function (event, toState) {
				var location = $location.url();
				if (location == $state.current.url && toState.name == 'rta') {
					if (toggles.RTA_FrontEndRefactor_44772) {
						var state = rtaStateStorageService.getState();
						if(!!state.state && !!state.params)
							$state.go(state.state, state.params);
						else $state.go('refact-rta');
					} else {
						$state.go('rta.sites')
					}
				}

			});
		return result;
	}
})();
