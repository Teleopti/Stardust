(function() {
	'use strict';

	angular.module('wfm.requests').config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('requests', {
			url: '/requests',
			controller: 'RequestsController'
		}).state('requestsOrigin', {
			templateUrl: 'app/requests/html/requests.html',
			controller: 'RequestsOriginCtrl as vm'
		}).state('requestsRefactor', {
			templateUrl: 'app/requests/html/requests.refactor.html',
			controller: 'RequestsRefactorCtrl as vm'
		}).state('requestsRefactor-absenceAndText', {
			parent: 'requestsRefactor',
			url: '/requests/absenceAndText',
			params: {
				agentSearchTerm: '',
				selectedGroupIds: [],
				filterEnabled: undefined,
				onInitCallBack: undefined,
				paging: {},
				isUsingRequestSubmitterTimeZone: undefined,
				getPeriod: undefined
			},
			templateUrl: 'app/requests/html/requests-absenceAndText.html',
			controller: 'requestsAbsenceAndTextCtrl as vm'
		}).state('requestsRefactor-shiftTrade', {
			parent: 'requestsRefactor',
			url: '/requests/shiftTrade',
			params: {
				agentSearchTerm: '',
				selectedGroupIds: [],
				filterEnabled: undefined,
				onInitCallBack: undefined,
				paging: {},
				isUsingRequestSubmitterTimeZone: undefined,
				getPeriod: undefined
			},
			templateUrl: 'app/requests/html/requests-shiftTrade.html',
			controller: 'requestsShiftTradeCtrl as vm'
		});
	}
})();