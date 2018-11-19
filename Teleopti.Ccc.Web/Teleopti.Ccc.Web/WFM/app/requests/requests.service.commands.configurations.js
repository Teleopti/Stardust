(function() {
	'use strict';

	angular
		.module('wfm.requests')
		.service('RequestsCommandsConfigurationsService', [
			'REQUESTS_TAB_NAMES',
			requestsCommandsConfigurationsService
		]);

	function requestsCommandsConfigurationsService(REQUESTS_TAB_NAMES) {
		var svc = this;
		svc.configurations = {};

		var ABSENCE_AND_TEXT = {
			approve: true,
			approveBasedOnBusinessRules: true,
			deny: true,
			cancel: true,
			processWaitlist: true,
			reply: true,
			siteOpenHour: true,
			viewAllowance: true
		};

		var SHIFT_TRADE = {
			approve: true,
			approveBasedOnBusinessRules: false,
			deny: true,
			cancel: false,
			processWaitlist: false,
			reply: false,
			siteOpenHour: true,
			viewAllowance: true
		};

		var OVERTIME = {
			approve: true,
			approveBasedOnBusinessRules: false,
			deny: true,
			cancel: false,
			processWaitlist: false,
			reply: false,
			siteOpenHour: true,
			viewAllowance: false
		};

		svc.configurations[REQUESTS_TAB_NAMES.absenceAndText] = ABSENCE_AND_TEXT;
		svc.configurations[REQUESTS_TAB_NAMES.shiftTrade] = SHIFT_TRADE;
		svc.configurations[REQUESTS_TAB_NAMES.overtime] = OVERTIME;

		return svc;
	}
})();
