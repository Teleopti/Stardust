(function () {
	angular
		.module('wfm.rta', [
			'wfm.rtaShared',
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
		.run(function ($rootScope, $injector) {

				$rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams, options) {
					$injector.invoke(function (rtaDataService, rtaStateService) {
                        if (toState.name == 'rta-agents')
                            rtaDataService.reload();
						if (fromState.name == 'rta-skill-area-manager')
							rtaDataService.reload();
						if (toState.name == 'rta' && fromState.name != 'rta')
							rtaStateService.gotoLastState();
					});
				})
			}
		);

})();
