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
			'wfm.helpingDirectives',
			'skillGroupService'
		])
		.run(function ($rootScope, rtaStateService, Toggle) {

			$rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams, options) {
				if (Toggle.RTA_RememberMyPartOfTheBusiness_39082)
					if (toState.name == 'rta' && fromState.name != 'rta')
						rtaStateService.gotoLastState();
			})

		});

})();
