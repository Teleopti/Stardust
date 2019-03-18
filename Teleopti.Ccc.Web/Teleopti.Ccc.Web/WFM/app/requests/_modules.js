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
			'wfm.utilities',
			'wfm.throttle'
		])
		.constant('REQUESTS_TAB_NAMES', {
			absenceAndText: 'absenceAndText',
			shiftTrade: 'shiftTrade',
			overtime: 'overtime'
		})
		.constant('REQUESTS_TYPES', {
			//refer to \Domain\InterfaceLegacy\Domain\RequestType.cs
			TextRequest: 0,
			AbsenceRequest: 1,
			ShiftTradeRequest: 2,
			ShiftExchangeOffer: 3,
			OvertimeRequest: 4
		})
		.constant('REQUESTS_STATUS', {
			//refer to \Domain\AgentInfo\Requests\PersonRequestStatus.cs
			Pending: 0,
			Denied: 1,
			Approved: 2,
			New: 3,
			AutoDenied: 4,
			Waitlisted: 5,
			Cancelled: 6
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
