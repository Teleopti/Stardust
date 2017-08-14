(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsAbsenceAndTextCtrl', requestsAbsenceAndTextCtrl);

	requestsAbsenceAndTextCtrl.$inject = [
		'$scope', '$filter', '$injector', '$translate', '$timeout', '$stateParams', 'requestsDataService', 'Toggle',
		'requestsNotificationService', 'uiGridConstants', 'requestsDefinitions', 'CurrentUserInfo', 'RequestsFilter', 'RequestGridStateService', 'TextAndAbsenceGridConfiguration',
		'UIGridUtilitiesService'
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
		uiGridUtilitiesService) {
		var vm = this;

		vm.requests = [];
		vm.period = {};
		vm.filters = [];
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.agentSearchTerm = '';
		vm.selectedTeamIds = [];
		vm.paging = {};
		vm.initialized = false;
		vm.requestFiltersMgr = new requestFilterSvc.RequestsFilter();

		var onInitCallBack = undefined,
			columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];

		vm.setDefaultStatuses = function() {
			if (!vm.showRequestsInDefaultStatus || vm.defaultStatusesLoaded) {
				return;
			}

			vm.SelectedRequestStatuses = uiGridUtilitiesService.getDefaultStatus(vm.filters, vm.requestFiltersMgr);
			vm.defaultStatusesLoaded = true;
		};

		vm.subjectFilterChanged = function() {
			vm.requestFiltersMgr.SetFilter('Subject', vm.subjectFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
		};

		vm.messageFilterChanged = function() {
			vm.requestFiltersMgr.SetFilter('Message', vm.messageFilter);
			vm.filters = vm.requestFiltersMgr.Filters;
		};

		vm.statusFilterClose = function () {
			setFilters(vm.SelectedRequestStatuses, 'Status');
		};

		vm.typeFilterClose = function() {
			setFilters(vm.SelectedTypes, 'Type');
		};

		vm.reload = function(params) {
			if (params) {
				vm.agentSearchTerm = params.agentSearchTerm || vm.agentSearchTerm;
				vm.selectedTeamIds = params.selectedTeamIds || vm.selectedTeamIds;
				vm.paging = params.paging || vm.paging;
			}

			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				selectedTeamIds: vm.selectedTeamIds,
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

			angular.forEach(vm.AllRequestStatuses,
				function (status) {
					status.Selected = false;
				});
			vm.SelectedRequestStatuses = [];

			vm.requestFiltersMgr.ResetFilter();
			vm.filters = vm.requestFiltersMgr.Filters;
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		}; 

		vm.init = function () {
			vm.defaultStatusesLoaded = false;
			vm.showRequestsInDefaultStatus = toggleService.Wfm_Requests_Default_Status_Filter_39472;
			vm.isUsingRequestSubmitterTimeZone = $stateParams.isUsingRequestSubmitterTimeZone;
			onInitCallBack = $stateParams.onInitCallBack;
			vm.userTimeZone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			vm.gridOptions = getGridOptions();
			vm.saveGridColumnState = toggleService.Wfm_Requests_Save_Grid_Columns_37976;
			if (toggleService.Wfm_Requests_Default_Status_Filter_39472) {
				vm.filters = [{ 'Status': '0 5' }];
			}
			vm.initialized = true;
			vm.filterEnabled = $stateParams.filterEnabled;
			vm.AllRequestStatuses = requestsDataService.getAllRequestStatuses(false);

			if (!$stateParams.getPeriod)
				return;

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
			vm.requestFiltersMgr.SetFilter(displayName, filters.trim());
			vm.filters = vm.requestFiltersMgr.Filters;
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
												vm.requestFiltersMgr.SetFilter(column.colDef.displayName, term.trim());
											}
										});
									vm.filters = vm.requestFiltersMgr.Filters;
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
			if (requestsFilter.selectedTeamIds.length === 0) {
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

			if (vm.saveGridColumnState) {
				initialiseGridStateHandling();
				vm.setDefaultStatuses();
			}

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			vm.gridOptions.data = vm.requests;
		}
	}
})();
