(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);


	requestsOverviewController.$inject = ['requestsDataService'];

	function requestsOverviewController(requestsDataService) {
		var vm = this;

		vm.requests = [];

		vm.period = {
			startDate: (vm.period && vm.period.startDate) ? vm.period.startDate : new Date(),
			endDate :  (vm.period && vm.period.endDate)? vm.period.endDate: new Date()
		};
		vm.period.endDate = vm.period.endDate? vm.period.endDate: new Date();

		vm.reload = reload;
		vm.sortingOrders = [];

		function reload(requestsFilter, sortingOrders) {
			vm.loaded = false;
			requestsDataService.getAllRequestsPromise(requestsFilter, sortingOrders).then(function (requests) {
				vm.requests = requests.data;
				vm.loaded = true;
			});
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {
				period: '='
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			scope.$watch(function() {
				var filter = {
					period: scope.requestsOverview.period
				}					
				return {
					startDate: filter.period.startDate,
					endDate: filter.period.endDate,
					sortingOrders: scope.requestsOverview.sortingOrders
				}
			}, function(newValue) {
				if (moment(newValue.endDate).isBefore(newValue.startDate, 'day')) return;				
				scope.requestsOverview.requestsFilter = newValue;
				ctrl.reload({ period: scope.requestsOverview.period }, scope.requestsOverview.sortingOrders);
			}, true);			
		}
	}
})();