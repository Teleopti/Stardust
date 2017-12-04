(function() {
	'use strict';

	angular.module('wfm.requests')
		.config(function($stateProvider) {
			$stateProvider.state('requests', {
				url: '/requests',
				templateUrl: 'app/requests/html/requests.html',
				controller: 'requestsRefactorController as vm'
			}).state('requests.absenceAndText', {
				url: '/absenceAndText',
				params: {
					getParams: undefined
				},
				templateUrl: 'app/requests/html/requests-absence-and-text.html',
				controller: 'requestsAbsenceAndTextController as vm'
			}).state('requests.shiftTrade', {
				url: '/shiftTrade',
				params: {
					getParams: undefined
				},
				templateUrl: 'app/requests/html/requests-shift-trade.html',
				controller: 'requestsShiftTradeController as vm'
			}).state('requests.overtime', {
				url: '/overtime',
				params: {
					getParams: undefined
				},
				templateUrl: 'app/requests/html/requests-overtime.html',
				controller: 'requestsOvertimeController as vm'
			});
		});
})();