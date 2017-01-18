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
		'wfm.modal',
		'wfm.favoriteSearch',
		'wfm.organizationPicker'
	]).run(moduleRun);

	moduleRun.$inject = ['$rootScope', 'organizationPickerSvc']; 
	function moduleRun($rootScope, organizationPickerSvc) {		
		$rootScope.$on('$stateChangeSuccess',
			function (event, toState) {
				if (toState.name === "requests") {
					organizationPickerSvc.setModule("wfm.requests");
				}
			});
	}

})();
