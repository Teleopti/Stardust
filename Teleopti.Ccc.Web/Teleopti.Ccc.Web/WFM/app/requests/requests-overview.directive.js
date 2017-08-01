(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);

	requestsOverviewController.$inject = ['$scope', "$attrs", 'requestsDataService', "Toggle", "requestsNotificationService"];

	function requestsOverviewController($scope, $attrs, requestsDataService, toggleService, requestsNotificationService) {
		var vm = this;

		vm.loadRequestWatchersInitialized = false;

		vm.requests = [];
		vm.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};

		vm.filters = [];
		vm.reload = reload;
		vm.isLoading = false;
		vm.sortingOrders = [];

		vm.init = init;
		vm.shiftTradeView = $attrs.shiftTradeView != undefined;

		function init() {
			vm.requestsPromise = vm.shiftTradeView ? requestsDataService.getShiftTradeRequestsPromise : requestsDataService.getAllRequestsPromise;
			vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
			// By default, show shift trade requests in pending only;
			// and show absence and text requests in pending and waitlisted only;
			if (toggleService.Wfm_Requests_Default_Status_Filter_39472) {
				vm.filters = [{ "Status": vm.shiftTradeView ? "0" : "0 5" }];
			}
		}

		toggleService.togglesLoaded.then(init);

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
			});
		}

		function reload(params) {
			if (!vm.isActive) {
				return;
			}
			if (params) {
				if (params.agentSearchTerm) vm.agentSearchTerm = params.agentSearchTerm;
				if (params.selectedTeamIds) vm.selectedTeamIds = params.selectedTeamIds;
				if (params.paging) vm.paging = params.paging;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedTeamIds: vm.selectedTeamIds,
				filters: vm.filters
			};

			vm.isLoading = true;
			if (vm.isPaginationEnabled) {
				getRequests(requestsFilter, vm.sortingOrders, vm.paging);
			} else {
				requestsDataService.getAllRequestsPromise_old(requestsFilter, vm.sortingOrders).then(function(requests) {
					vm.requests = requests.data;
					vm.isLoading = false;
				});
			}
		}
	}

	function validateDateParameters(startDate, endDate) {
		if (endDate === null || startDate === null) return false;
		return !(moment(endDate).isBefore(startDate, 'day')) && moment(startDate).year() > 1969 && moment(endDate).year() > 1969;
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {
				period: '=?',
				agentSearchTerm: '=?',
				selectedTeamIds: '=?',
				filters: '=?',
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
				scope.$broadcast('reload.requests.without.selection');

				if (!ctrl.loadRequestWatchersInitialized) {
					listenToReload();
				}
			}, true);

			function listenToReload() {
				scope.$on('reload.requests.with.selection', function (event, data) {
					if((!angular.isArray(vm.selectedTeamIds) || vm.selectedTeamIds.length == 0) && angular.isUndefined(data)){
						return;
					}

					ctrl.reload(data);
				});

				scope.$on('reload.requests.without.selection', function (event) {
					ctrl.reload();
				});

				ctrl.loadRequestWatchersInitialized = true;
			}
		}
	}
})();
