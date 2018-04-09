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
					if (toState.name == 'rta' && fromState.name != 'rta') {
						// Invoke to delay service creation
						$injector.invoke(function (rtaStateService) {
							rtaStateService.gotoLastState();
						})
					}
				})

			}
		);

})();
