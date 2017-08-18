(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsShiftTradeCtrl', requestsShiftTradeCtrl);

	requestsShiftTradeCtrl.$inject = [
		'$scope', '$filter', '$injector', '$translate', '$timeout', '$stateParams', 'requestsDataService', 'Toggle',
		'requestsNotificationService', 'uiGridConstants', 'requestsDefinitions', 'CurrentUserInfo', 'RequestsFilter', 'RequestGridStateService', 'ShiftTradeGridConfiguration',
		'UIGridUtilitiesService'
	];

	function requestsShiftTradeCtrl($scope,
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
		shiftTradeGridConfiguration,
		uiGridUtilitiesService) {
		var vm = this;

		vm.requests = [];
		vm.period = {};
		vm.filters = [];
		vm.isLoading = false;
		vm.sortingOrders = [];
		vm.agentSearchTerm = '';
		vm.selectedGroupIds = [];
		vm.paging = {};
		vm.initialized = false;
		vm.shiftTradeView = true;
		vm.requestFiltersMgr = new requestFilterSvc.RequestsFilter();

		var onInitCallBack = undefined,
			columnsWithFilterEnabled = ['Subject', 'Message', 'Type', 'Status'];

		vm.setDefaultStatuses = function() {
			if (vm.defaultStatusesLoaded) {
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

		vm.statusFilterClose = function() {
			setFilters(vm.SelectedRequestStatuses, 'Status');
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

		vm.clearAllFilters = function() {
			angular.forEach(vm.gridApi.grid.columns,
				function(column) {
					column.filters[0].term = undefined;
				});
			vm.SelectedTypes = [];

			angular.forEach(vm.AllRequestStatuses,
				function(status) {
					status.Selected = false;
				});
			vm.SelectedRequestStatuses = [];

			vm.requestFiltersMgr.ResetFilter();
			vm.filters = vm.requestFiltersMgr.Filters;
			vm.subjectFilter = undefined;
			vm.messageFilter = undefined;
		};

		vm.showShiftDetail = function(params) {
			vm.schedules = params.schedules;
			vm.shiftDetailTop = params.top;
			vm.shiftDetailLeft = params.left;
			vm.displayShiftDetail = true;
		};

		vm.hideShiftDetail = function () {
			vm.displayShiftDetail = false;
		};

		vm.shiftDetailStyleJson = function() {
			return {
				top: vm.shiftDetailTop + 'px',
				left: vm.shiftDetailLeft + 'px',
				position: 'fixed'
			};
		};

		vm.isDayOff = function(scheduleDayDetail) {
			return (scheduleDayDetail && (scheduleDayDetail.Type === requestsDefinitions.SHIFT_OBJECT_TYPE.DayOff));
		};

		vm.shouldDisplayShiftTradeDayDetail = function(shiftTradeDayDetail) {
			return (shiftTradeDayDetail && (shiftTradeDayDetail.ShortName != null && shiftTradeDayDetail.ShortName.length !== 0));
		};

		vm.getShiftTradeHeaderClass = function() {
			return vm.shiftTradeView || vm.filterEnabled ? 'request-header-full-height' : '';
		};

		vm.init = function() {
			vm.defaultStatusesLoaded = false;
			vm.isUsingRequestSubmitterTimeZone = $stateParams.isUsingRequestSubmitterTimeZone;
			onInitCallBack = $stateParams.onInitCallBack;
			vm.userTimeZone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			var sortingOrder = requestsDefinitions.translateSingleSortingOrder(requestGridStateService.getShiftTradeSorting());
			if(sortingOrder)
				vm.sortingOrders.push(sortingOrder);

			vm.gridOptions = getGridOptions();
			vm.saveGridColumnState = toggleService.Wfm_Requests_Save_Grid_Columns_37976;
			vm.filters = [{
				'Status': '0'
			}];
			vm.initialized = true;
			vm.filterEnabled = $stateParams.filterEnabled;
			vm.AllRequestStatuses = requestsDataService.getAllRequestStatuses(vm.shiftTradeView);

			if (!$stateParams.getPeriod)
				return;

			setupWatch();
		};

		toggleService.togglesLoaded.then(vm.init);

		$scope.$on('reload.requests.with.selection',
			function(event, data) {
				vm.initialized && vm.reload(data);
			});

		$scope.$on('requests.filterEnabled.changed',
			function(event, data) {
				vm.filterEnabled = data;
				vm.gridOptions.enableFiltering = vm.filterEnabled;
				vm.gridOptions.useExternalFiltering = vm.filterEnabled;
				angular.forEach(vm.gridOptions.columnDefs, function(col) {
					col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
				});
				vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			});

		$scope.$on('requests.isUsingRequestSubmitterTimeZone.changed',
			function(event, data) {
				vm.isUsingRequestSubmitterTimeZone = data;
				prepareComputedColumns(vm.requests);
			});

		function setFilters(filtersList, displayName) {
			var filters = '';
			angular.forEach(filtersList,
				function(filter) {
					filters += filter.Id + ' ';
				});
			vm.requestFiltersMgr.SetFilter(displayName, filters.trim());
			vm.filters = vm.requestFiltersMgr.Filters;
		}

		function setupWatch() {
			$scope.$watch(function() {
					return {
						period: $stateParams.getPeriod(),
						filters: vm.filters,
						sortingOrder: vm.sortingOrders
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
				headerTemplate: 'app/requests/html/requests-shift-trade-header.html',
				gridMenuTitleFilter: $translate,
				columnVirtualizationThreshold: 200,
				rowHeight: 60,

				onRegisterApi: function(gridApi) {
					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function(grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function(x) {
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
			var visibleRequestsIds = vm.gridOptions.data.map(function(row) {
				return row.Id;
			});
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function(row) {
				return row.Id;
			});
			var messages = vm.gridApi.selection.getSelectedRows().map(function(row) {
				return row.Message;
			});
			uiGridUtilitiesService.setShiftTradeSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, messages);
			vm.gridApi.grid.selection.selectAll = vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0;
		}

		function getRequests(requestsFilter, sortingOrders, paging) {
			if (requestsFilter.selectedGroupIds.length === 0) {
				getRequestsCallback({
					data: {
						Requests: []
					}
				});
			} else {
				vm.isLoading = true;
				requestsDataService.getShiftTradeRequestsPromise(requestsFilter, sortingOrders, paging).then(getRequestsCallback);
			}
		}

		function getRequestsCallback(requests) {
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
				};
			}

			prepareComputedColumns(vm.requests);

			onInitCallBack && onInitCallBack(requests.data.TotalCount);
			vm.isLoading = false;
		}

		function prepareComputedColumns(requests) {
			uiGridUtilitiesService.prepareComputedColumns(requests, vm.userTimeZone, vm.isUsingRequestSubmitterTimeZone);

			setupShiftTradeVisualisation(requests);

			vm.gridOptions.columnDefs = shiftTradeGridConfiguration.columnDefinitions(vm.shiftTradeRequestDateSummary);
			angular.forEach(vm.gridOptions.columnDefs, function(col) {
				col.enableFiltering = vm.filterEnabled && columnsWithFilterEnabled.indexOf(col.displayName) > -1;
			});

			vm.gridOptions.enableFiltering = vm.filterEnabled;
			vm.gridOptions.enablePinning = true;
			vm.gridOptions.useExternalFiltering = vm.filterEnabled;
			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

			if (vm.saveGridColumnState) {
				initialiseGridStateHandling();
				vm.setDefaultStatuses();
			}

			vm.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
			vm.gridOptions.data = vm.requests;
		}

		function setupShiftTradeVisualisation(requests) {
			if (!vm.shiftTradeView || !vm.shiftTradeRequestDateSummary) return;

			vm.shiftTradeDayViewModels = shiftTradeGridConfiguration.getDayViewModels(requests,
				vm.shiftTradeRequestDateSummary, vm.isUsingRequestSubmitterTimeZone);

			vm.shiftTradeScheduleViewModels = shiftTradeGridConfiguration.getShiftTradeScheduleViewModels(requests,
				vm.shiftTradeRequestDateSummary, vm.isUsingRequestSubmitterTimeZone);
		}
	}
})();