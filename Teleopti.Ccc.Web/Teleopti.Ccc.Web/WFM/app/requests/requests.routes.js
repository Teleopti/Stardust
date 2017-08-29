(function() {
	'use strict';

	angular
		.module('wfm.requests')
		.provider('RequestsState', function() {
			var toggles = {}

			this.$get = function() {
				return function(toggleService) {
					toggleService.togglesLoaded.then(function() {
						toggles = toggleService
					});
				};
			};

			this.config = function($stateProvider) {
				$stateProvider.state('requests', {
					url: '/requests',
					templateUrl: function() {
						if (toggles.Wfm_Requests_Refactoring_45470)
							return 'app/requests/html/requests.refactor.html';
						else
							return 'app/requests/html/requests.html';
					},
					controllerProvider: function(){
						if(toggles.Wfm_Requests_Refactoring_45470)
							return 'RequestsRefactorCtrl as vm';
						else
							return 'RequestsOriginCtrl as vm';
					}
				});

				$stateProvider.state('requests.absenceAndText', {
					url: '/absenceAndText',
					params: {
						getParams: undefined
					},
					templateUrl: 'app/requests/html/requests-absenceAndText.html',
					controller: 'requestsAbsenceAndTextCtrl as vm'
				}).state('requests.shiftTrade', {
					url: '/shiftTrade',
					params: {
						getParams: undefined
					},
					templateUrl: 'app/requests/html/requests-shiftTrade.html',
					controller: 'requestsShiftTradeCtrl as vm'
				}).state('requests.overtime', {
					url: '/overtime',
					params: {
						getParams: undefined
					},
					templateUrl: 'app/requests/html/requests-overtime.html',
					controller: 'requestsOvertimeCtrl as vm'
				});
			};
		});
})();