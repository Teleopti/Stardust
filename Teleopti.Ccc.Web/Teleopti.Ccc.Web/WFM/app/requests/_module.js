(function () {

	'use strict';

	angular.module('wfm.requests', [
		'ui.grid',
		'ui.grid.autoResize',
		'ui.grid.selection',
		'ui.grid.pinning',
		'ui.grid.saveState',
		'pascalprecht.translate',
		'wfm.notice',
		'currentUserInfoService',
        'wfm.signalR',
        'wfm.teamSchedule',
		'wfm.multiplesearchinput',
		'wfm.favoriteSearch',
		'wfm.organizationPicker',
		'wfm.inputFocus'
	]).run(moduleRun);

	moduleRun.$inject = ['$rootScope', 'organizationPickerSvc', 'FavoriteSearchDataService'];
	function moduleRun($rootScope, organizationPickerSvc, FavoriteSearchDataService) {
		$rootScope.$on('$stateChangeSuccess',
			function (event, toState) {
				if (toState.name === "requests") {
					organizationPickerSvc.setModule("wfm.requests");
					FavoriteSearchDataService.setModule("wfm.requests");
				}
			});
	}

})();
