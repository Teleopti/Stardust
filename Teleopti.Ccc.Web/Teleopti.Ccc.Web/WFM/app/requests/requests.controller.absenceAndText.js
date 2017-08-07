(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsAbsenceAndTextCtrl', requestsAbsenceAndTextCtrl);

	requestsAbsenceAndTextCtrl.$inject = ['$scope','$stateParams', 'requestsDataService', "Toggle", "requestsNotificationService"];

	function requestsAbsenceAndTextCtrl($scope, $stateParams,requestsDataService, toggleService, requestsNotificationService) {
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

		function init() {
			vm.requestsPromise = requestsDataService.getAllRequestsPromise;
			vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
			if (toggleService.Wfm_Requests_Default_Status_Filter_39472) {
				vm.filters = [{ "Status": "0 5" }];
			}
			vm.reload();
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
				vm.onInitCallBack && vm.onInitCallBack({ count: requests.data.TotalCount });

				vm.isLoading = false;
			});
		}

		function reload() {
			if ($stateParams) {
				vm.agentSearchTerm = $stateParams.agentSearchTerm;
				vm.selectedTeamIds = $stateParams.selectedTeamIds;
				vm.paging = $stateParams.paging;
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
})();
