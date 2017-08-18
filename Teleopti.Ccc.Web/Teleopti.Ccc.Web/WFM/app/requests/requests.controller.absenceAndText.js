(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsAbsenceAndTextCtrl', requestsAbsenceAndTextCtrl);

	requestsAbsenceAndTextCtrl.$inject = [
		'$scope', '$filter', '$injector', '$translate', '$timeout', '$stateParams', 'requestsDataService', 'Toggle',
		'requestsNotificationService', 'uiGridConstants', 'requestsDefinitions', 'CurrentUserInfo', 'RequestsFilter', 'RequestGridStateService', 'TextAndAbsenceGridConfiguration', 'UIGridUtilitiesService', 'REQUESTS_TAB_NAMES'
	];

	function requestsAbsenceAndTextCtrl($scope,
		$filter,
		$injector,
		$translate,
		$timeout,
		$stateParams,
		requestsDataService,
		toggleService,
		requestsNotificationService,
		uiGridConstants,
		requestsDefinitions,
		currentUserInfo,
		requestFilterSvc,
		requestGridStateService,
		textAndAbsenceGridConfigurationService,
		uiGridUtilitiesService, 
		requestsTabNames) {
		var vm = this;

		vm.requests = [];
		vm.period = {};
		vm.filters = [];
		vm.subjectFilter = undefined;
		vm.messageFilter = undefined;
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.agentSearchTerm = '';
		vm.selectedGroupIds = [];
		vm.paging = {};
		vm.initialized = false;

		var onInitCallBack = undefined,
			columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];

		vm.setDefaultStatuses = function() {
			if (vm.defaultStatusesLoaded) {
				return;
			}

			vm.selectedRequestStatuses = uiGridUtilitiesService.getDefaultStatus(vm.filters, requestsTabNames.absenceAndText);
			vm.defaultStatusesLoaded = true;
		};

		vm.subjectFilterChanged = function() {
			requestFilterSvc.setFilter('Subject', vm.subjectFilter, requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		};

		vm.messageFilterChanged = function() {
			requestFilterSvc.setFilter('Message', vm.messageFilter, requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		};

		vm.statusFilterClose = function () {
			setFilters(vm.selectedRequestStatuses, 'Status');
		};

		vm.typeFilterClose = function() {
			setFilters(vm.SelectedTypes, 'Type');
		};

		vm.reload = function(params) {
			if (params) {
				vm.agentSearchTerm = params.agentSearchTerm || vm.agentSearchTerm;
				vm.selectedGroupIds = params.selectedGroupIds || vm.selectedGroupIds;
				vm.paging = params.paging || vm.paging;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedGroupIds: vm.selectedGroupIds,
				filters: vm.filters
			};


			getRequests(requestsFilter, vm.sortingOrders, vm.paging);
		};

		vm.clearAllFilters = function () {
			angular.forEach(vm.gridApi.grid.columns,
				function (column) {
					column.filters[0].term = undefined;
				});

			angular.forEach(vm.AllRequestableAbsences,
				function (absence) {
					absence.Selected = false;
				});
			vm.SelectedTypes = [];

			angular.forEach(vm.allRequestStatuses,
				function (status) {
					status.Selected = false;
				});
			vm.selectedRequestStatuses = [];

			requestFilterSvc.resetFilter(requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		}; 

		vm.init = function () {
			vm.defaultStatusesLoaded = false;
			vm.isUsingRequestSubmitterTimeZone = $stateParams.isUsingRequestSubmitterTimeZone;
			onInitCallBack = $stateParams.onInitCallBack;
			vm.userTimeZone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			var sortingOrder = requestsDefinitions.translateSingleSortingOrder(requestGridStateService.getAbsenceAndTextSorting());
			if(sortingOrder)
				vm.sortingOrders.push(sortingOrder);

			vm.gridOptions = getGridOptions();
			vm.initialized = true;
			vm.filterEnabled = $stateParams.filterEnabled;
			vm.allRequestStatuses = requestsDataService.getAllRequestStatuses(false);

			if (!$stateParams.getPeriod)
				return;

			if(requestFilterSvc.filters[requestsTabNames.absenceAndText]){
				vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];

				var subjectFilter = vm.filters.filter(function(f){return Object.keys(f)[0] == 'Subject';})[0];
				var messageFilter = vm.filters.filter(function(f){return Object.keys(f)[0] == 'Message';})[0];

				if(subjectFilter)
					vm.subjectFilter = subjectFilter['Subject'];
				if(messageFilter)
					vm.messageFilter = messageFilter['Message'];
			} else{
				vm.filters = [{ 'Status': '0 5' }];
			}

			getRequestTypes();
			setupWatch();
		};

		toggleService.togglesLoaded.then(vm.init);

		$scope.$on('reload.requests.with.selection',
			function (event, data) {
				vm.initialized && vm.reload(data);
			});

		$scope.$on('requests.filterEnabled.changed',
			function (event, data) {
				vm.filterEnabled = data;
				vm.gridOptions.enableFiltering = vm.filterEnabled;
				vm.gridOptions.useExternalFiltering = vm.filterEnabled;
				angular.forEach(vm.gridOptions.columnDefs, function (col) {
					col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
				});
				vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			});

		$scope.$on('requests.isUsingRequestSubmitterTimeZone.changed',
			function (event, data) {
				vm.isUsingRequestSubmitterTimeZone = data;
				prepareComputedColumns(vm.requests);
			});

		function setFilters(filtersList, displayName) {
			var filters = '';
			angular.forEach(filtersList,
				function (filter) {
					filters += filter.Id + ' ';
				});
			requestFilterSvc.setFilter(displayName, filters.trim(), requestsTabNames.absenceAndText);
			vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
		}

		function setupWatch() {
			$scope.$watch(function() {
					return{
						period: $stateParams.getPeriod(),
						filters: vm.filters,
						sortOrder: vm.sortingOrders
					};
				},
				function (newVal) {
					if (!newVal || !vm.initialized)
						return;
					if (validateDateParameters(newVal.period.startDate, newVal.period.endDate)) {
						vm.period = newVal.period;
						vm.reload($stateParams);
					}
				}, true);
		}

		function getRequestTypes() {
			requestsDataService.getRequestTypes().then(function(result) {
				vm.AllRequestTypes = result.data;
				angular.forEach(vm.AllRequestTypes,
					function(type) {
						type.Selected = false;
					});
			});
		}

		function initialiseGridStateHandling() {
			vm.shiftTradeView = false;
			requestGridStateService.restoreState(vm);
			// delay the setup of these handlers a little to let the table load
			$timeout(function() {
				requestGridStateService.setupGridEventHandlers($scope, vm);
			}, 500);
		}

		function validateDateParameters(startDate, endDate) {
			if (endDate === null || startDate === null) return false;
			return !(moment(endDate).isBefore(startDate, 'day')) && moment(startDate).year() > 1969 && moment(endDate).year() > 1969;
		}

		function getGridOptions() {
			var options = {
				appScopeProvider: vm,
				enableGridMenu: true,
				enableFullRowSelection: true,
				enableRowHeaderSelection: true,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				enableVerticalScrollbar: uiGridConstants.scrollbars.WHEN_NEEDED,
				useExternalSorting: true,
				headerTemplate: 'app/requests/html/requests-absence-and-text-header.html',
				gridMenuTitleFilter: $translate,
				columnVirtualizationThreshold: 200,
				rowHeight: 35,

				onRegisterApi: function(gridApi) {
					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
							vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder)
								.filter(function(x) { return x !== null; });
						});
					gridApi.grid.clearAllFilters = vm.clearAllFilters;
					gridApi.selection.on.rowSelectionChanged($scope, onSelectionChanged);
					gridApi.selection.on.rowSelectionChangedBatch($scope, onSelectionChanged);

					var filterHandlingTimeout = null;
					gridApi.core.on.filterChanged($scope, function () {
							var grid = this.grid;

							if (filterHandlingTimeout != null) {
								$timeout.cancel(filterHandlingTimeout);
							}

							filterHandlingTimeout = $timeout(function() {
									angular.forEach(grid.columns,
										function(column) {
											var term = column.filters[0].term;
											if (term != undefined) {
												requestFilterSvc.setFilter(column.colDef.displayName, term.trim(), requestsTabNames.absenceAndText);
											}
										});
									vm.filters = requestFilterSvc.filters[requestsTabNames.absenceAndText];
								},
								500);
						});
				}
			};

			return options;
		}

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function(row) { return row.Id; });
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Id; });
			var messages = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Message; });
			uiGridUtilitiesService.setAbsenceAndTextSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, messages);
			vm.gridApi.grid.selection.selectAll = vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0;
		}

		function getRequests(requestsFilter, sortingOrders, paging) {
			if (requestsFilter.selectedGroupIds.length === 0) {
				getRequestsCallback({
					data: {
						Requests:[]
					}
				});
			} else {
				vm.isLoading = true;
				requestsDataService.getAllRequestsPromise(requestsFilter, sortingOrders, paging).then(getRequestsCallback);
			}
		}

		function getRequestsCallback(requests) {
			if (!requests) {
				vm.isLoading = false;
				return;
			}

			vm.requests = requests.data.Requests;
			prepareComputedColumns(vm.requests);

			if (requests.data.IsSearchPersonCountExceeded) {
				vm.requests = [];
				requestsNotificationService.notifyMaxSearchPersonCountExceeded(requests.data.MaxSearchPersonCount);
			}
			onInitCallBack && onInitCallBack(requests.data.TotalCount);
			vm.isLoading = false;
		}

		function prepareComputedColumns(requests) {
			uiGridUtilitiesService.prepareComputedColumns(requests, vm.userTimeZone, vm.isUsingRequestSubmitterTimeZone);

			vm.gridOptions.columnDefs = textAndAbsenceGridConfigurationService.columnDefinitions();
			angular.forEach(vm.gridOptions.columnDefs, function (col) {
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});

			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

			initialiseGridStateHandling();
			vm.setDefaultStatuses();

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			vm.gridOptions.data = vm.requests;
		}
	}
})();
