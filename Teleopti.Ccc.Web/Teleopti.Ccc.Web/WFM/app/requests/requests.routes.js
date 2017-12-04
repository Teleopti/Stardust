(function() {
	'use strict';

	angular
		.module('wfm.requests')
		.provider('RequestsState', function() {
			var toggles = {}

			this.$get = function() {
				return function(toggleService) {
					toggleService.togglesLoaded.then(function() {
						toggles = toggleService;
					});
				};
			};

			this.config = function($stateProvider) {
				$stateProvider.state('requests', {
					url: '/requests',
					templateUrl: function() {
						return 'app/requests/html/requests.refactor.html';
					},
					controllerProvider: function(){
						return 'requestsRefactorController as vm';
					}
				});

				$stateProvider.state('requests.absenceAndText', {
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
			};
		});
})();