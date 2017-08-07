(function () {
	'use strict';

	angular.module('wfm.requests').config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('requests',
			{
				url: '/requests',
				templateUrl: 'app/requests/html/requests.html',
				controller: 'RequestsCtrl as vm'
			}).state('requestsRefactor',
			{
				url: '/requests-refactor',
				templateUrl: 'app/requests/html/requests.refactor.html',
				controller: 'RequestsRefactorCtrl as vm'
			}).state('requestsRefactor.absenceAndText',
			{
				url: '/absenceAndText',
				params: {
					period: undefined,
					agentSearchTerm: '',
					selectedTeamIds: [],
					filterEnabled: undefined,
					onInitCallBack: undefined,
					paging: {},
					isUsingRequestSubmitterTimeZone: undefined
				},
				templateUrl: 'app/requests/html/requests-absenceAndText.html',
				controller: 'requestsAbsenceAndTextCtrl as vm'
			}).state('requestsRefactor.shiftTrade',
			{
				url: '/shiftTrade',
				templateUrl: 'app/requests/html/requests-shiftTrade.html',
				controller: 'requestsShiftTradeCtrl as vm'
			});
	}
})();
