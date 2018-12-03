(function() {
	'use strict';

	angular
		.module('wfm.requests', [
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
			'wfm.ngEnter',
			'wfm.utilities'
		])
		.constant('REQUESTS_TAB_NAMES', {
			absenceAndText: 'absenceAndText',
			shiftTrade: 'shiftTrade',
			overtime: 'overtime'
		})
		.constant('REQUESTS_TYPES', {
			TextRequest: 0,
			AbsenceRequest: 1,
			ShiftTradeRequest: 2,
			ShiftExchangeOffer: 3,
			OvertimeRequest: 4
		})
		.run(moduleRun);

	moduleRun.$inject = ['$rootScope', 'FavoriteSearchDataService', 'groupPageService'];
	function moduleRun($rootScope, FavoriteSearchDataService, GroupPageService) {
		$rootScope.$on('$stateChangeSuccess', function(event, toState) {
			if (toState.name.indexOf('requests') > -1) {
				FavoriteSearchDataService.setModule('wfm.requests');
				GroupPageService.setModule('wfm.requests');
			}
		});
	}
})();
