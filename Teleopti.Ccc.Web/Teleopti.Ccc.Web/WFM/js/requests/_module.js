(function () {

	'use strict';

	angular.module('wfm.requests', [
		'ui.grid',
		'ui.grid.autoResize',
		'ui.grid.selection',
		'ui.grid.pinning',
		'pascalprecht.translate',
		'wfm.notice',
		'currentUserInfoService',
		'isteven-multi-select',
        'wfm.signalR',
        'wfm.teamSchedule'
	]);

})();
