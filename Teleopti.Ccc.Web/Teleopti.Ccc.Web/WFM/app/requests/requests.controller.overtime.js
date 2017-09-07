(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOvertimeController', requestsOvertimeController);

	requestsOvertimeController.$inject = [
		'$scope', '$filter', '$injector', '$translate', '$timeout', '$stateParams', 'requestsDataService', 'Toggle',
		'requestsNotificationService', 'uiGridConstants', 'requestsDefinitions', 'CurrentUserInfo', 'RequestsFilter', 'RequestGridStateService', 'OvertimeGridConfiguration', 'UIGridUtilitiesService', 'REQUESTS_TAB_NAMES'
	];

	function requestsOvertimeController($scope, $filter, $injector, $translate, $timeout, $stateParams, requestsDataService, toggleService, requestsNotificationService, uiGridConstants, requestsDefinitions, currentUserInfo, requestFilterSvc, requestGridStateService, overtimeGridConfigurationService, uiGridUtilitiesService, requestsTabNames) {

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

			vm.selectedRequestStatuses = uiGridUtilitiesService.getDefaultStatus(vm.filters, requestsTabNames.overtime);
			vm.defaultStatusesLoaded = true;
		};

		vm.subjectFilterChanged = function() {
			requestFilterSvc.setFilter('Subject', vm.subjectFilter, requestsTabNames.overtime);
			vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];
		};

		vm.messageFilterChanged = function() {
			requestFilterSvc.setFilter('Message', vm.messageFilter, requestsTabNames.overtime);
			vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];
		};

		vm.statusFilterClose = function () {
			setFilters(vm.selectedRequestStatuses, 'Status');
		};

		vm.typeFilterClose = function() {
			setFilters(vm.selectedTypes, 'Type');
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

			angular.forEach(vm.overtimeTypes,
				function (overtime) {
					overtime.Selected = false;
				});
			vm.selectedTypes = [];

			angular.forEach(vm.allRequestStatuses,
				function (status) {
					status.Selected = false;
				});
			vm.selectedRequestStatuses = [];

			requestFilterSvc.resetFilter(requestsTabNames.overtime);
			vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		}; 

		vm.init = function () {
			vm.defaultStatusesLoaded = false;
			vm.userTimeZone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			var sortingOrder = requestsDefinitions.translateSingleSortingOrder(requestGridStateService.getOvertimeSorting());

			if(sortingOrder)
				vm.sortingOrders.push(sortingOrder);

			vm.gridOptions = getGridOptions();
			vm.allRequestStatuses = requestsDataService.getOvertimeRequestsStatuses();

			var params = $stateParams.getParams && $stateParams.getParams();
			if (!params)
				return;

			vm.filterEnabled = params.filterEnabled;
			vm.isUsingRequestSubmitterTimeZone = params.isUsingRequestSubmitterTimeZone;
			onInitCallBack = params.onInitCallBack;

			if(requestFilterSvc.filters[requestsTabNames.overtime]){
				vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];

				var subjectFilter = vm.filters.filter(function(f){return Object.keys(f)[0] == 'Subject';})[0];
				var messageFilter = vm.filters.filter(function(f){return Object.keys(f)[0] == 'Message';})[0];

				if(subjectFilter)
					vm.subjectFilter = subjectFilter['Subject'];
				if(messageFilter)
					vm.messageFilter = messageFilter['Message'];
			} else{
				vm.filters = [{ 'Status': '0' }];
			}

			getOvertimeTypes();
			applyGridColumns();

			vm.initialized = true;
			setupWatch();
		};

		toggleService.togglesLoaded.then(vm.init);

		$scope.$on('reload.requests.with.selection', function(event, data) {
			vm.initialized && vm.reload(data);
		});

		$scope.$on('reload.requests.without.selection', function() {
			vm.initialized && vm.reload();
		});

		$scope.$on('requests.filterEnabled.changed', function(event, data) {
			vm.filterEnabled = data;
			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;
			angular.forEach(vm.gridOptions.columnDefs, function(col) {
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
		});

		$scope.$on('requests.isUsingRequestSubmitterTimeZone.changed', function(event, data) {
			vm.isUsingRequestSubmitterTimeZone = data;
			prepareComputedColumns(vm.requests);
		});

		function setFilters(filtersList, displayName) {
			var filters = '';
			angular.forEach(filtersList,
				function (filter) {
					filters += filter.Id + ' ';
				});
			requestFilterSvc.setFilter(displayName, filters.trim(), requestsTabNames.overtime);
			vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];
		}

		function setupWatch() {
			$scope.$watch(function() {
					return{
						period: $stateParams.getParams().getPeriod(),
						filters: vm.filters,
						sortOrder: vm.sortingOrders
					};
				},
				function (newVal) {
					if (!newVal || !vm.initialized)
						return;
					if (validateDateParameters(newVal.period.startDate, newVal.period.endDate)) {
						vm.period = newVal.period;
						vm.reload($stateParams.getParams());
					}
				}, true);
		}

		function getOvertimeTypes() {
			requestsDataService.getOvertimeTypes().then(function(result) {
				vm.overtimeTypes = result.data;
				angular.forEach(vm.OvertimeTypes,
					function(type) {
						type.Selected = false;
					});
			});
		}

		function initialiseGridStateHandling() {
			vm.shiftTradeView = false;
			requestGridStateService.restoreState(vm, requestsDefinitions.REQUEST_TYPES.OVERTIME);
			// delay the setup of these handlers a little to let the table load
			$timeout(function() {
				requestGridStateService.setupGridEventHandlers($scope, vm, requestsDefinitions.REQUEST_TYPES.OVERTIME);
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
					gridApi.core.on.sortChanged($scope, function(grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder)
							.filter(function(x) {
								return x !== null;
							});
					});
					gridApi.grid.clearAllFilters = vm.clearAllFilters;
					gridApi.selection.on.rowSelectionChanged($scope, onSelectionChanged);
					gridApi.selection.on.rowSelectionChangedBatch($scope, onSelectionChanged);

					var filterHandlingTimeout = null;
					gridApi.core.on.filterChanged($scope, function() {
						var grid = this.grid;

						if (filterHandlingTimeout != null) {
							$timeout.cancel(filterHandlingTimeout);
						}

						filterHandlingTimeout = $timeout(function() {
								angular.forEach(grid.columns,
									function(column) {
										var term = column.filters[0].term;
										if (angular.isDefind(term)) {
											requestFilterSvc.setFilter(column.colDef.displayName, term.trim(), requestsTabNames.overtime);
										}
									});
								vm.filters = requestFilterSvc.filters[requestsTabNames.overtime];
							},
							500);
					});
				}
			};

			return options;
		}

		function applyGridColumns(){
			vm.gridOptions.columnDefs = overtimeGridConfigurationService.columnDefinitions();
			angular.forEach(vm.gridOptions.columnDefs, function (col) {
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});
		}

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function(row) { return row.Id; });
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Id; });
			var messages = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Message; });
			uiGridUtilitiesService.setOvertimeSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, messages);
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
				requestsDataService.getOvertimeRequestsPromise(requestsFilter, sortingOrders, paging).then(getRequestsCallback);
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
			vm.gridApi.grid.selection.selectAll = false;
		}

		function prepareComputedColumns(requests) {
			uiGridUtilitiesService.prepareComputedColumns(requests, vm.userTimeZone, vm.isUsingRequestSubmitterTimeZone);

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
