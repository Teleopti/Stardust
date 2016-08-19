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
        'wfm.teamSchedule'
	]);

})();
