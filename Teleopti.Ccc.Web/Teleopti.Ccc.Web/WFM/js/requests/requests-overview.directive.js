﻿(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);

	requestsOverviewController.$inject = ['$scope', 'requestsDataService'];

	function requestsOverviewController($scope, requestsDataService) {
		var vm = this;

		vm.requests = [];
		vm.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};
		vm.agentSearchTerm = "";
		vm.filters = [];
		vm.period.endDate = moment().endOf('week')._d;
		vm.reload = reload;
		vm.sortingOrders = [];

		function reload(requestsFilter, sortingOrders, paging, done) {
			vm.loaded = false;

			if (vm.togglePaginationEnabled) {
				requestsDataService.getAllRequestsPromise(requestsFilter, sortingOrders, paging).then(function(requests) {
					vm.requests = requests.data.Requests;
					if (vm.totalRequestsCount !== requests.data.TotalCount) {
						vm.totalRequestsCount = requests.data.TotalCount;
						if (typeof vm.onTotalRequestsCountChanges == 'function')
							vm.onTotalRequestsCountChanges({ totalRequestsCount: vm.totalRequestsCount });
					}
					vm.loaded = true;
					if (done != null) done();
				});

			} else {
				requestsDataService.getAllRequestsPromise_old(requestsFilter, sortingOrders).then(function(requests) {
					vm.requests = requests.data;
					vm.loaded = true;
				});
				if (done != null) done();
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
				filters: '=?',
				paging: '=?',
				onTotalRequestsCountChanges: '&?',
				togglePaginationEnabled: '=?'
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			scope.$watch(function() {
				var target = {
					startDate: scope.requestsOverview.period ? scope.requestsOverview.period.startDate : null,
					endDate: scope.requestsOverview.period ? scope.requestsOverview.period.endDate : null,
					agentSearchTerm: scope.requestsOverview.agentSearchTerm ? scope.requestsOverview.agentSearchTerm : '',
					filters: scope.requestsOverview.filters ? scope.requestsOverview.filters : ''
				};
				return target;
			}, function(newValue) {
				if (newValue.endDate === null || newValue.startDate === null) return;
				if (moment(newValue.endDate).isBefore(newValue.startDate, 'day')) return;
				scope.$broadcast('reload.requests.without.selection');
				listenToReload();
			}, true);

			scope.$watch(function() {
				return scope.requestsOverview.sortingOrders;
			}, reload());

			function listenToReload() {
				scope.$on('reload.requests.with.selection', reload());
				scope.$on('reload.requests.without.selection', reload());
			}

			function reload(done) {
				return function() {
					ctrl.reload({
						period: scope.requestsOverview.period,
						agentSearchTerm: scope.requestsOverview.agentSearchTerm,
						filters: scope.requestsOverview.filters
					}, scope.requestsOverview.sortingOrders, scope.requestsOverview.paging, done);
				};
			}
		}
	}
})();
