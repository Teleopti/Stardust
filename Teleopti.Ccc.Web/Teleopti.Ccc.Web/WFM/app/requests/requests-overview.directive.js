(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewController', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);

	requestsOverviewController.$inject = ['$scope', "$attrs", 'requestsDataService', "Toggle", "requestsNotificationService", "RequestsFilter", "REQUESTS_TAB_NAMES"];

	function requestsOverviewController($scope, $attrs, requestsDataService, toggleService, requestsNotificationService, requestFilterSvc, requestsTabNames) {
		var vm = this;

		vm.loadRequestWatchersInitialized = false;

		vm.requests = [];
		vm.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};

		vm.filters = [];
		vm.reload = reload;
		vm.firstLoadedInitialized = false;
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.shiftTradeView = angular.isDefined($attrs.shiftTradeView);

		vm.init = function() {
			vm.requestsPromise = vm.shiftTradeView ? requestsDataService.getShiftTradeRequestsPromise : requestsDataService.getAbsenceAndTextRequestsPromise;
			// By default, show shift trade requests in pending only;
			// and show absence and text requests in pending and waitlisted only;
			var tabName = vm.shiftTradeView ? requestsTabNames.shiftTrade : requestsTabNames.absenceAndText;

			if(requestFilterSvc.filters[tabName]){
				vm.filters = requestFilterSvc.filters[tabName];
			} else{
				vm.filters = [{ "Status": vm.shiftTradeView ? "0" : "0 5" }];
			}
		}

		toggleService.togglesLoaded.then(vm.init);

		function getRequests(requestsFilter, sortingOrders, paging) {
			vm.requestsPromise(requestsFilter, sortingOrders, paging).then(function (requests) {
				if (!requests) {
					vm.isLoading = false;
					return;
				}

				vm.requests = requests.data.Requests;

				if (requests.data.IsSearchPersonCountExceeded) {
					vm.requests = [];
					requestsNotificationService.notifyMaxSearchPersonCountExceeded(requests.data.MaxSearchPersonCount);
				} else if (vm.requests && vm.requests.length > 0) {
					vm.shiftTradeRequestDateSummary = {
						Minimum: requests.data.MinimumDateTime,
						Maximum: requests.data.MaximumDateTime,
						FirstDayOfWeek: requests.data.FirstDayOfWeek
					}
				}

				vm.totalRequestsCount = requests.data.TotalCount;
				vm.onInitCallBack({ count: requests.data.TotalCount });

				vm.isLoading = false;
				vm.firstLoadedInitialized = true;
			});
		}

		function reload(params) {
			if (!vm.isActive) {
				return;
			}
			if (params) {
				if (params.agentSearchTerm) vm.agentSearchTerm = params.agentSearchTerm;
				if (params.selectedGroupIds) vm.selectedGroupIds = params.selectedGroupIds;
				if (params.paging) vm.paging = params.paging;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedGroupIds: vm.selectedGroupIds,
				selectedGroupPageId: vm.selectedGroupPageId,
				filters: vm.filters
			};

			if(requestsFilter.selectedGroupIds.length == 0) return;

			vm.isLoading = true;
			getRequests(requestsFilter, vm.sortingOrders, vm.paging);
		}
	}

	function validateDateParameters(startDate, endDate) {
		if (endDate === null || startDate === null) return false;
		return !(moment(endDate).isBefore(startDate, 'day')) && moment(startDate).year() > 1969 && moment(endDate).year() > 1969;
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewController',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {
				period: '=?',
				agentSearchTerm: '=?',
				selectedGroupIds: '=?',
				selectedGroupPageId: '=?',
				filterEnabled: '=?',
				isActive: '=?',
				onInitCallBack: '&?',
				paging: '=?',
				isUsingRequestSubmitterTimeZone: '=?'
			},
			restrict: 'E',
			templateUrl: 'app/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			var vm = scope.requestsOverview;
			scope.$watch(function() {
				var target = {
					startDate: vm.period ? vm.period.startDate : null,
					endDate: vm.period ? vm.period.endDate : null,
					filters: vm.filters,
					isActive: vm.isActive,
					sortingOrders: vm.sortingOrders
				};
				return target;
			}, function (newValue, oldValue) {
				if (!newValue || !validateDateParameters(newValue.startDate, newValue.endDate)) {
					return;
				}

				if(vm.firstLoadedInitialized || vm.shiftTradeView)
					scope.$broadcast('reload.requests.without.selection');

				if (!ctrl.loadRequestWatchersInitialized) {
					listenToReload();
				}
			}, true);

			function listenToReload() {
				scope.$on('reload.requests.with.selection', function (event, data) {
					if((!angular.isArray(vm.selectedGroupIds) || vm.selectedGroupIds.length == 0) && angular.isUndefined(data)){
						return;
					}
					ctrl.reload(data);
				});

				scope.$on('reload.requests.without.selection', function (event, data) {
					ctrl.reload();
				});

				ctrl.loadRequestWatchersInitialized = true;
			}
		}
	}
})();
