(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);


	requestsOverviewController.$inject = ['$scope','requestsDataService'];

	function requestsOverviewController($scope,requestsDataService) {
		var vm = this;

		vm.requests = [];	
		vm.period = {
			startDate: (vm.period && vm.period.startDate) ? vm.period.startDate : new Date(),
			endDate: (vm.period && vm.period.endDate) ? vm.period.endDate : new Date()
		};
		vm.agentSearchTerm = "";
		vm.period.endDate = vm.period.endDate ? vm.period.endDate : new Date();

		vm.reload = reload;
		vm.sortingOrders = [];

	
		function reload(requestsFilter, sortingOrders, paging) {
			vm.loaded = false;

			if (vm.togglePaginationEnabled) {
				requestsDataService.getAllRequestsPromise(requestsFilter, sortingOrders, paging).then(function (requests) {
					vm.requests = requests.data.Requests;
					if (vm.totalRequestsCount !== requests.data.TotalCount) {						
						vm.totalRequestsCount = requests.data.TotalCount;						
						if (typeof vm.onTotalRequestsCountChanges == 'function')
							vm.onTotalRequestsCountChanges({ totalRequestsCount: vm.totalRequestsCount });
					}					
					vm.loaded = true;
				});

			} else {
				requestsDataService.getAllRequestsPromise_old(requestsFilter, sortingOrders).then(function (requests) {
					vm.requests = requests.data;
					vm.loaded = true;
				});
			}			
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {
				period: '=',
				agentSearchTerm: '=?',
				paging: '=?',
				onTotalRequestsCountChanges: '&?',
				togglePaginationEnabled: '=?'
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {

			scope.$watch(function () {				
				var target = {
					startDate: scope.requestsOverview.period.startDate,
					endDate: scope.requestsOverview.period.endDate,			
					agentSearchTerm: scope.requestsOverview.agentSearchTerm ? scope.requestsOverview.agentSearchTerm : ""
				}				
				return target;
			}, function (newValue) {
				if (newValue.endDate === null || newValue.startDate === null) return;
				if (moment(newValue.endDate).isBefore(newValue.startDate, 'day')) return;
				scope.requestsOverview.requestsFilter = newValue;
				reload();
				scope.$broadcast('reload.requests.without.selection');
			}, true);

			scope.$watch(function() {
				return scope.requestsOverview.sortingOrders;
			}, reload);

			scope.$on('reload.requests.with.selection', reload);
			scope.$on('reload.requests.without.selection', reload);

			function reload() {							
				ctrl.reload({
					period: scope.requestsOverview.period,
					agentSearchTerm: scope.requestsOverview.agentSearchTerm,
				}, scope.requestsOverview.sortingOrders, scope.requestsOverview.paging);								
			}
		}
	}
})();